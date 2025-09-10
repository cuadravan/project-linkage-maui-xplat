using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PLinkage.Interfaces;
using PLinkage.Models;

namespace PLinkage.ViewModels
{
    public partial class SendMessageViewModel : ObservableObject
    {
        // Services
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISessionService _sessionService;
        private readonly INavigationService _navigationService;
        private Guid _senderId = Guid.Empty;
        private Guid _receiverId = Guid.Empty;

        // Properties
        [ObservableProperty] private IUser sender;
        [ObservableProperty] private IUser receiver;
        [ObservableProperty] private string receiverFullName; // Bind to full name label
        [ObservableProperty] private string messageContent; // Bind to content

        public SendMessageViewModel(IUnitOfWork unitOfWork, ISessionService sessionService, INavigationService navigationService)
        {
            _unitOfWork = unitOfWork;
            _sessionService = sessionService;
            _navigationService = navigationService;
            LoadDetailsCommand = new AsyncRelayCommand(LoadDetails);
        }
        public IAsyncRelayCommand LoadDetailsCommand { get; }

        private async Task LoadDetails()
        {
            await _unitOfWork.ReloadAsync();

            _receiverId = _sessionService.VisitingReceiverID;
            _senderId = _sessionService.GetCurrentUser().UserId;

            // Load sender (current logged-in user)
            var currentUser = _sessionService.GetCurrentUser();
            Sender = currentUser; // IUser is base type, this works

            // Try finding receiver in SkillProvider repository
            var skillProviderReceiver = await _unitOfWork.SkillProvider.GetByIdAsync(_receiverId);
            if (skillProviderReceiver != null)
            {
                Receiver = skillProviderReceiver;
            }
            else
            {
                // Try ProjectOwner repository
                var projectOwnerReceiver = await _unitOfWork.ProjectOwner.GetByIdAsync(_receiverId);
                if (projectOwnerReceiver != null)
                {
                    Receiver = projectOwnerReceiver;
                }
                else
                {
                    // Try Admin repository last
                    var adminReceiver = await _unitOfWork.Admin.GetByIdAsync(_receiverId);
                    if (adminReceiver != null)
                    {
                        Receiver = adminReceiver;
                    }
                }
            }

            if (Receiver != null)
            {
                ReceiverFullName = $"{Receiver.UserFirstName} {Receiver.UserLastName}";
            }
        }

        [RelayCommand]
        private async Task SendMessage()
        {
            if (string.IsNullOrWhiteSpace(MessageContent))
            {
                await Shell.Current.DisplayAlert("❗ Missing Info", "You cannot send an empty message.", "OK");
                return;
            }

            await _unitOfWork.ReloadAsync();

            var allChat = await _unitOfWork.Chat.GetAllAsync();

            var existingChat = allChat.FirstOrDefault(chat =>
                chat.MessengerId.Count == 2 &&
                chat.MessengerId.Contains(_senderId) &&
                chat.MessengerId.Contains(_receiverId)
            );

            Chat targetChat;

            if (existingChat != null)
            {
                // Append message to existing chat
                var newMessage = new Message
                {
                    SenderId = _senderId,
                    ReceiverId = _receiverId,
                    MessageOrder = existingChat.Messages.Count + 1,
                    MessageContent = MessageContent,
                    MessageDate = DateTime.Now
                };
                existingChat.Messages.Add(newMessage);
                await _unitOfWork.Chat.UpdateAsync(existingChat);
                targetChat = existingChat;
            }
            else
            {
                // Create a new chat
                var newChat = new Chat
                {
                    MessengerId = new List<Guid> { _senderId, _receiverId },
                    Messages = new List<Message>
                    {
                        new Message
                        {
                            SenderId = _senderId,
                            ReceiverId = _receiverId,
                            MessageOrder = 1,
                            MessageContent = MessageContent,
                            MessageDate = DateTime.Now
                        }
                    }
                };
                await _unitOfWork.Chat.AddAsync(newChat);
                targetChat = newChat;
            }

            // Ensure both users have this chat ID in their message list
            await AddChatIdToUserAsync(_senderId, targetChat.ChatId);
            await AddChatIdToUserAsync(_receiverId, targetChat.ChatId);
            await _unitOfWork.SaveChangesAsync();

            await Shell.Current.DisplayAlert("Success", "Message sent successfully!", "OK");
            await GoBack();
        }

        private async Task AddChatIdToUserAsync(Guid userId, Guid chatId)
        {
            // Try SkillProvider first
            var skillProvider = await _unitOfWork.SkillProvider.GetByIdAsync(userId);
            if (skillProvider != null)
            {
                if (!skillProvider.UserMessagesId.Contains(chatId))
                {
                    skillProvider.UserMessagesId.Add(chatId);
                    await _unitOfWork.SkillProvider.UpdateAsync(skillProvider);
                }
                return;
            }

            // Try ProjectOwner
            var projectOwner = await _unitOfWork.ProjectOwner.GetByIdAsync(userId);
            if (projectOwner != null)
            {
                if (!projectOwner.UserMessagesId.Contains(chatId))
                {
                    projectOwner.UserMessagesId.Add(chatId);
                    await _unitOfWork.ProjectOwner.UpdateAsync(projectOwner);
                }
                return;
            }

            // Try Admin last
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

        [RelayCommand]
        private async Task GoBack()
        {
            await _navigationService.GoBackAsync();
        }
    }
}
