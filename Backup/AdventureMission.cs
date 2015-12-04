using System;

public class AdventureMission
{
    private string m_description;
    private WingProgress m_grantedProgress;
    private WingProgress m_requiredProgress;
    private int m_scenarioID;

    public AdventureMission(int scenarioID, string description, WingProgress requiredProgress, WingProgress grantedProgress)
    {
        this.m_scenarioID = scenarioID;
        this.m_description = description;
        this.m_requiredProgress = !requiredProgress.IsEmpty() ? requiredProgress : null;
        this.m_grantedProgress = !grantedProgress.IsEmpty() ? grantedProgress : null;
    }

    public bool HasGrantedProgress()
    {
        return (this.m_grantedProgress != null);
    }

    public bool HasRequiredProgress()
    {
        return (this.m_requiredProgress != null);
    }

    public override string ToString()
    {
        object[] args = new object[] { this.ScenarioID, this.Description, this.RequiredProgress, this.GrantedProgress };
        return string.Format("[AdventureMission: ScenarioID={0}, Description={1} RequiredProgress={2} GrantedProgress={3}]", args);
    }

    public string Description
    {
        get
        {
            return this.m_description;
        }
    }

    public WingProgress GrantedProgress
    {
        get
        {
            return this.m_grantedProgress;
        }
    }

    public WingProgress RequiredProgress
    {
        get
        {
            return this.m_requiredProgress;
        }
    }

    public int ScenarioID
    {
        get
        {
            return this.m_scenarioID;
        }
    }

    public class WingProgress
    {
        private ulong m_flags;
        private int m_progress;
        private int m_wing;

        public WingProgress(int wing, int progress, ulong flags)
        {
            this.m_wing = wing;
            this.m_progress = progress;
            this.m_flags = flags;
        }

        public AdventureMission.WingProgress Clone()
        {
            return new AdventureMission.WingProgress(this.Wing, this.Progress, this.Flags);
        }

        public bool IsEmpty()
        {
            if (this.Wing == 0)
            {
                return true;
            }
            if (this.Progress > 0)
            {
                return false;
            }
            return (this.Flags == 0L);
        }

        public bool IsOwned()
        {
            return this.MeetsFlagsRequirement(1L);
        }

        public bool MeetsFlagsRequirement(ulong requiredFlags)
        {
            return ((this.Flags & requiredFlags) == requiredFlags);
        }

        public bool MeetsProgressAndFlagsRequirements(AdventureMission.WingProgress requiredProgress)
        {
            if (requiredProgress == null)
            {
                return true;
            }
            if (requiredProgress.Wing != this.Wing)
            {
                return false;
            }
            return this.MeetsProgressAndFlagsRequirements(requiredProgress.Progress, requiredProgress.Flags);
        }

        public bool MeetsProgressAndFlagsRequirements(int requiredProgress, ulong requiredFlags)
        {
            return (this.MeetsProgressRequirement(requiredProgress) && this.MeetsFlagsRequirement(requiredFlags));
        }

        public bool MeetsProgressRequirement(int requiredProgress)
        {
            return (this.Progress >= requiredProgress);
        }

        public void SetFlags(ulong flags)
        {
            this.m_flags = flags;
        }

        public void SetProgress(int progress)
        {
            if (this.m_progress <= progress)
            {
                this.m_progress = progress;
            }
        }

        public override string ToString()
        {
            return string.Format("[AdventureMission.WingProgress: Wing={0}, Progress={1} Flags={2}]", this.Wing, this.Progress, this.Flags);
        }

        public ulong Flags
        {
            get
            {
                return this.m_flags;
            }
        }

        public int Progress
        {
            get
            {
                return this.m_progress;
            }
        }

        public int Wing
        {
            get
            {
                return this.m_wing;
            }
        }
    }
}

