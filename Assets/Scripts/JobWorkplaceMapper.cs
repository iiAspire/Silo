public static class JobWorkplaceMapper
{
    public static WorkplaceType[] GetWorkplaceTypesForJob(string job)
    {
        switch (job)
        {
            case "Engineer":
            case "Mechanic":
            case "Engineers Shadow":
            case "Mechanics Shadow":
                return new[] { WorkplaceType.Generator };

            case "Cook":
            case "Cooks Shadow":
                return new[] { WorkplaceType.Canteen };

            case "Janitor":
            case "Janitors Shadow":
                return new[] { WorkplaceType.JanitorOffice };

            case "Guard":
            case "Guards Shadow":
                return new[] { WorkplaceType.Security };

            case "Farmer":
            case "Farmers Shadow":
                return new[] { WorkplaceType.Farm };

            case "Doctor":
            case "Doctors Shadow":
                return new[] { WorkplaceType.Clinic };

            case "Maintenance Worker":
            case "Maintenance Workers Shadow":
                return new[] { WorkplaceType.Maintenance };

            case "Student":
            case "Teacher":
            case "Teachers Shadow":
                return new[] { WorkplaceType.School };

            case "Judge":
            case "Judges Shadow":
                return new[] { WorkplaceType.JudgesChambers };

            case "Mayor":
            case "Mayors Shadow":
                return new[] { WorkplaceType.MayorsOffice };

            case "Sheriff":
            case "Deputy":
            case "Sheriffs Shadow":
            case "Deputys Shadow":
                return new[] { WorkplaceType.SheriffStation };

            case "Prisoner":
            case "Cleaner":
                return new[] { WorkplaceType.Prison };

            case "Butcher":
            case "Butchers Shadow":
                return new[] { WorkplaceType.Butcher };

            case "Baker":
            case "Bakers Shadow":
                return new[] { WorkplaceType.Baker };

            case "Builder":
            case "Builders Shadow":
                return new[] { WorkplaceType.Builder };

            case "Carpenter":
            case "Carpenters Shadow":
                return new[] { WorkplaceType.Carpenter };

            case "Programmer":
            case "IT Technician":
            case "Programmers Shadow":
            case "IT Technicians Shadow":
            case "Head of IT":
            case "Head of ITs Shadow":
                return new[] { WorkplaceType.IT };

            case "Market Trader":
            case "Market Traders Shadow":
                return new[] { WorkplaceType.Bazaar };

            case "Surgeon":
            case "Medical Technician":
            case "Surgeons Shadow":
            case "Medical Technicians Shadow":
                return new[] { WorkplaceType.Hospital };

            case "Porter":
            case "Porters Shadow":
                return new[] { WorkplaceType.PorterHub };

            case "Factory Worker":
            case "Factory Workers Shadow":
                return new[] { WorkplaceType.Manufacturing };

            case "Machinist":
            case "Machinists Shadow":
                return new[] { WorkplaceType.Processing };

            case "Supply Worker":
            case "Supply Workers Shadow":
                return new[] { WorkplaceType.Supply };

            case "Waste Management Worker":
            case "Waste Management Workers Shadow":
                return new[] { WorkplaceType.WasteManagement };

            case "Recycling Worker":
            case "Recycling Workers Shadow":
                return new[] { WorkplaceType.Recycling };

            case "Administrator":
                return new[]
                {
                    WorkplaceType.Clinic,
                    WorkplaceType.Hospital,
                    WorkplaceType.PorterHub
                };

            case "Administrators Shadow":
                return new[]
                {
                    WorkplaceType.Clinic,
                    WorkplaceType.Hospital,
                    WorkplaceType.PorterHub
                };

            case "Clerk":
                return new[]
                {
                    WorkplaceType.JudgesChambers,
                    WorkplaceType.MayorsOffice
                };

            case "Clerks Shadow":
                return new[]
                {
                    WorkplaceType.JudgesChambers,
                    WorkplaceType.MayorsOffice
                };

            case "Nurse":
                return new[]
                {
                    WorkplaceType.Nursery,
                    WorkplaceType.Hospital
                };

            case "Nurses Shadow":
                return new[]
                {
                    WorkplaceType.Nursery,
                    WorkplaceType.Hospital
                };

            default:
                return new[] { WorkplaceType.None };
        }
    }
}