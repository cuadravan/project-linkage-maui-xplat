using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using PLinkage.Interfaces;
using PLinkage.Models;
using CommunityToolkit.Mvvm.Input;

namespace PLinkage.ViewModels
{
    public partial class ViewMessagesViewModel : ObservableObject
    {
        // Services
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISessionService _sessionService;
        private readonly INavigationService _navigationService;

        [ObservableProperty]
        private ObservableCollection<ChatSummaryViewModel> chatSummaries;

        [ObservableProperty]
        private ObservableCollection<ChatMessageViewModel> selectedChatMessages;

        [ObservableProperty]
        private ChatSummaryViewModel selectedChat;

        [ObservableProperty]
        private string messageContent;

        private Guid _currentUserId;

        public ViewMessagesViewModel(IUnitOfWork unitOfWork, ISessionService sessionService, INavigationService navigationService)
        {
            _unitOfWork = unitOfWork;
            _sessionService = sessionService;
            _navigationService = navigationService;
            _currentUserId = _sessionService.GetCurrentUser().UserId;

            LoadChatSummariesCommand = new AsyncRelayCommand(LoadChatSummariesAsync);
            ChatSelectedCommand = new AsyncRelayCommand<ChatSummaryViewModel>(OnChatSelectedAsync);
        }
        public IAsyncRelayCommand LoadChatSummariesCommand { get; }
        public IAsyncRelayCommand<ChatSummaryViewModel> ChatSelectedCommand { get; }

        private async Task LoadChatSummariesAsync()
        {
            await _unitOfWork.ReloadAsync();
            SelectedChatMessages = new ObservableCollection<ChatMessageViewModel>();
            var currentUser = _sessionService.GetCurrentUser();
            var allChats = await _unitOfWork.Chat.GetAllAsync();

            var summaries = new List<ChatSummaryViewModel>();

            foreach (var chat in allChats)
            {
                if (!chat.MessengerId.Contains(currentUser.UserId))
                    continue;

                // Get the other user
                var receiverId = chat.MessengerId.First(id => id != currentUser.UserId);

                // Load name from SkillProvider, ProjectOwner, or Admin repositories
                string fullName = string.Empty;
                var sp = await _unitOfWork.SkillProvider.GetByIdAsync(receiverId);
                if (sp != null)
                    fullName = $"{sp.UserFirstName} {sp.UserLastName}";
                else
                {
                    var po = await _unitOfWork.ProjectOwner.GetByIdAsync(receiverId);
                    if (po != null)
                        fullName = $"{po.UserFirstName} {po.UserLastName}";
                    else
                    {
                        var admin = await _unitOfWork.Admin.GetByIdAsync(receiverId);
                        if (admin != null)
                            fullName = $"{admin.UserFirstName} {admin.UserLastName}";
                    }
                }

                // Get latest message
                var latest = chat.Messages.OrderByDescending(m => m.MessageDate).FirstOrDefault();

                summaries.Add(new ChatSummaryViewModel
                {
                    ChatId = chat.ChatId,
                    ReceiverFullName = fullName,
                    MostRecentMessage = latest?.MessageContent ?? "(No message)",
                    MessageDate = latest?.MessageDate ?? DateTime.MinValue
                });
            }

            // Sort by most recent message
            ChatSummaries = new ObservableCollection<ChatSummaryViewModel>(
                summaries.OrderByDescending(s => s.MessageDate)
            );
        }

        private async Task OnChatSelectedAsync(ChatSummaryViewModel selected)
        {
            if (selected == null) return;

            var chat = await _unitOfWork.Chat.GetByIdAsync(selected.ChatId);
            if (chat == null) return;

            var messages = chat.Messages
                .OrderBy(m => m.MessageDate)
                .Select(m => new ChatMessageViewModel
                {
                    Content = m.MessageContent,
                    Date = m.MessageDate,
                    IsFromCurrentUser = m.SenderId == _currentUserId
                })
                .ToList();

            // Mark unread messages sent to current user as read
            bool updated = false;
            foreach (var msg in chat.Messages)
            {
                if (!msg.IsRead && msg.ReceiverId == _currentUserId)
                {
                    msg.IsRead = true;
                    updated = true;
                }
            }

            if (updated)
                await _unitOfWork.Chat.UpdateAsync(chat);

            SelectedChat = selected;
            SelectedChatMessages = new ObservableCollection<ChatMessageViewModel>(messages);
        }

        [RelayCommand]
        private async Task SendMessageAsync()
        {
            if (SelectedChat == null)
            {
                await Shell.Current.DisplayAlert("❗ No Chat Selected", "Please select a conversation first.", "OK");
                return;
            }

            if (string.IsNullOrWhiteSpace(MessageContent))
            {
                await Shell.Current.DisplayAlert("❗ Empty Message", "Please type a message before sending.", "OK");
                return;
            }

            var chat = await _unitOfWork.Chat.GetByIdAsync(SelectedChat.ChatId);
            if (chat == null)
            {
                await Shell.Current.DisplayAlert("❌ Error", "Chat not found.", "OK");
                return;
            }

            var receiverId = chat.MessengerId.First(id => id != _currentUserId);

            var newMessage = new Message
            {
                SenderId = _currentUserId,
                ReceiverId = receiverId,
                MessageContent = MessageContent,
                MessageOrder = chat.Messages.Count + 1,
                MessageDate = DateTime.Now
            };

            chat.Messages.Add(newMessage);
            await _unitOfWork.Chat.UpdateAsync(chat);

            await AddChatIdToUserAsync(_currentUserId, chat.ChatId);
            await AddChatIdToUserAsync(receiverId, chat.ChatId);
            await _unitOfWork.SaveChangesAsync();

            // Clear input
            MessageContent = string.Empty;

            // Update UI
            SelectedChatMessages.Add(new ChatMessageViewModel
            {
                Content = newMessage.MessageContent,
                Date = newMessage.MessageDate,
                IsFromCurrentUser = true
            });

            // Update MostRecentMessage in summary (optional)
            var summary = ChatSummaries.FirstOrDefault(c => c.ChatId == chat.ChatId);
            if (summary != null)
            {
                summary.MostRecentMessage = newMessage.MessageContent;
                summary.MessageDate = newMessage.MessageDate;
            }
        }

        private async Task AddChatIdToUserAsync(Guid userId, Guid chatId)
        {
            var sp = await _unitOfWork.SkillProvider.GetByIdAsync(userId);
            if (sp != null)
            {
                if (!sp.UserMessagesId.Contains(chatId))
                {
                    sp.UserMessagesId.Add(chatId);
                    await _unitOfWork.SkillProvider.UpdateAsync(sp);
                }
                return;
            }

            var po = await _unitOfWork.ProjectOwner.GetByIdAsync(userId);
            if (po != null)
            {
                if (!po.UserMessagesId.Contains(chatId))
                {
                    po.UserMessagesId.Add(chatId);
                    await _unitOfWork.ProjectOwner.UpdateAsync(po);
                }
                return;
            }

            var admin = await _unitOfWork.Admin.GetByIdAsync(userId);
            if (admin != null)
            {
                if (!admin.UserMessagesId.Contains(chatId))
                {
                    admin.UserMessagesId.Add(chatId);
                    await _unitOfWork.Admin.UpdateAsync(admin);
                }
            }
        }
    }

    public class ChatSummaryViewModel
    {
        public Guid ChatId { get; set; }
        public string ReceiverFullName { get; set; }
        public string MostRecentMessage { get; set; }
        public DateTime MessageDate { get; set; } // For sorting
    }
    public class ChatMessageViewModel
    {
        public string Content { get; set; }
        public DateTime Date { get; set; }
        public bool IsFromCurrentUser { get; set; }
    }

}
