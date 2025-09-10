namespace PLinkage.Models
{
    public class Skill
    {
        public string SkillName { get; set; } = string.Empty;
        public string SkillDescription { get; set; } = string.Empty;
        public int SkillLevel { get; set; } // 0-5
        public DateTime TimeAcquired { get; set; } = DateTime.Now;
        public string OrganizationInvolved { get; set; } = string.Empty;
        public int YearsOfExperience { get; set; }
    }
}
