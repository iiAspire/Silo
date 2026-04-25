using System;
using UnityEngine;

public static class JobWorkplaceMapper
{
    public static WorkplaceType[] GetWorkplaceTypesForJob(string baseJob)
    {
        if (string.IsNullOrWhiteSpace(baseJob))
            return new[] { WorkplaceType.None };

        switch (baseJob.Trim())
        {
            case "Engineer":
            case "Mechanic":
                return new[] { WorkplaceType.Generator };

            case "Security Officer":
                return new[] { WorkplaceType.SecurityOffice };

            case "Cook":
                return new[] { WorkplaceType.Canteen };

            case "Janitor":
                return new[] { WorkplaceType.JanitorOffice };

            case "Guard":
                return new[] { WorkplaceType.Security };

            case "Farmer":
                return new[] { WorkplaceType.Farm };

            case "Doctor":
                return new[] { WorkplaceType.Clinic };

            case "Maintenance Worker":
                return new[] { WorkplaceType.Maintenance };

            case "Student":
            case "Teacher":
                return new[] { WorkplaceType.School };

            case "Judge":
                return new[] { WorkplaceType.JudgesChambers };

            case "Mayor":
                return new[] { WorkplaceType.MayorsOffice };

            case "Sheriff":
            case "Deputy":
                return new[] { WorkplaceType.SheriffStation };

            case "Prisoner":
            case "Cleaner":
                return new[] { WorkplaceType.Prison };

            case "Butcher":
                return new[] { WorkplaceType.Butcher };

            case "Baker":
                return new[] { WorkplaceType.Baker };

            case "Builder":
                return new[] { WorkplaceType.Builder };

            case "Carpenter":
                return new[] { WorkplaceType.Carpenter };

            case "Programmer":
            case "IT Technician":
            case "Head of IT":
                return new[] { WorkplaceType.IT };

            case "Market Trader":
                return new[] { WorkplaceType.Bazaar };

            case "Surgeon":
            case "Medical Technician":
                return new[] { WorkplaceType.Hospital };

            case "Porter":
                return new[] { WorkplaceType.PorterHub };

            case "Factory Worker":
                return new[] { WorkplaceType.Manufacturing };

            case "Machinist":
                return new[] { WorkplaceType.Processing };

            case "Supply Worker":
                return new[] { WorkplaceType.Supply };

            case "Waste Management Worker":
                return new[] { WorkplaceType.WasteManagement };

            case "Recycling Worker":
                return new[] { WorkplaceType.Recycling };

            case "Administrator":
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

            case "Nurse":
                return new[]
                {
                    WorkplaceType.Hospital
                };

            case "Nursery Nurse":
                return new[]
                {
                    WorkplaceType.Nursery
                };

            default:
                Debug.LogWarning($"No workplace mapping found for base job '{baseJob}'.");
                return new[] { WorkplaceType.None };
        }
    }
}