namespace PLinkage.Models
{
    public class Project
    {
        public Guid ProjectId { get; set; } = Guid.NewGuid();
        public Guid ProjectOwnerId { get; set; } = Guid.Empty;
        public string ProjectName { get; set; } = string.Empty;
        public CebuLocation? ProjectLocation { get; set; } = null;
        public string ProjectDescription { get; set; } = string.Empty;
        public DateTime ProjectStartDate { get; set; } = DateTime.Now;
        public DateTime ProjectEndDate { get; set; } = DateTime.Now;
        public ProjectStatus? ProjectStatus { get; set; } = null;
        public List<string> ProjectSkillsRequired { get; set; } = new List<string>();
        public List<ProjectMemberDetail> ProjectMembers { get; set; } = new List<ProjectMemberDetail>();
        public string ProjectPriority { get; set; } = string.Empty;
        public int ProjectResourcesNeeded { get; set; } = 0;
        public int ProjectResourcesAvailable { get; set; } = 0;
        public DateTime ProjectDateCreated { get; set; } = DateTime.Now;
        public DateTime ProjectDateUpdated { get; set; } = DateTime.Now;
    }
}

public enum ProjectStatus
{
    Active,
    Completed,
    Deactivated,
}

public class ProjectMemberDetail
{
    public Guid MemberId { get; set; }
    public string UserFirstName { get; set; } // From SkillProvider
    public string UserLastName { get; set; } // From SkillProvider
    public string Email { get; set; } // From SkillProvider
    public decimal Rate { get; set; } = 0; // e.g. 1000 per hour
    public int TimeFrame { get; set; } = 0; // Hours
    public bool IsResigning { get; set; } = false;
    public string? ResignationReason { get; set; } = string.Empty; // Reason for resignation
}
