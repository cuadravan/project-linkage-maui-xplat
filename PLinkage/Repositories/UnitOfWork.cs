using PLinkage.Interfaces;
using PLinkage.Models;

namespace PLinkage.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        public IRepository<Admin> Admin { get; }
        public IRepository<ProjectOwner> ProjectOwner { get; }
        public IRepository<SkillProvider> SkillProvider { get; }
        public IRepository<Project> Projects { get; }
        public IRepository<Chat> Chat { get; }
        public IRepository<OfferApplication> OfferApplications { get; }

        public UnitOfWork()
        {
            Admin = new JsonRepository<Admin>("Admin");
            ProjectOwner = new JsonRepository<ProjectOwner>("ProjectOwner");
            SkillProvider = new JsonRepository<SkillProvider>("SkillProvider");
            Projects = new JsonRepository<Project>("Projects");
            Chat = new JsonRepository<Chat>("Chat");
            OfferApplications = new JsonRepository<OfferApplication>("OfferApplications");
        }

        public async Task SaveChangesAsync()
        {
            await Admin.SaveChangesAsync();
            await ProjectOwner.SaveChangesAsync();
            await SkillProvider.SaveChangesAsync();
            await Projects.SaveChangesAsync();
            await Chat.SaveChangesAsync();
            await OfferApplications.SaveChangesAsync();
        }

        public async Task ReloadAsync()
        {
            Admin.Reload();
            ProjectOwner.Reload();
            SkillProvider.Reload();
            Projects.Reload();
            Chat.Reload();
            OfferApplications.Reload();

            await Task.CompletedTask;
        }
    }
}
