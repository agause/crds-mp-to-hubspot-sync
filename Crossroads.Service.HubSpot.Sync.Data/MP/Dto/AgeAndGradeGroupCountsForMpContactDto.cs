﻿
namespace Crossroads.Service.HubSpot.Sync.Data.MP.Dto
{
    /// <summary>
    /// Kids Club and Student Ministry Age and Grade 
    /// </summary>
    public class AgeAndGradeGroupCountsForMpContactDto : IDeveloperIntegrationProperties, ICoreContactProperties, IAgeGradeContactProperties
    {
        public string MinistryPlatformContactId { get; set; }

        public string Source => "MP_Sync_Kids_Club_&_Student_Ministry_Update";

        public string Email { get; set; }

        public string Firstname { get; set; }

        public string Lastname { get; set; }

        public string Marital_Status { get; set; }

        public string Gender { get; set; }

        public string Community { get; set; }

        public string Phone { get; set; }

        public string MobilePhone { get; set; }

        public string Zip { get; set; }

        public int Number_of_Infants { get; set; }

        public int Number_of_1_Year_Olds { get; set; }

        public int Number_of_2_Year_Olds { get; set; }

        public int Number_of_3_Year_Olds { get; set; }

        public int Number_of_4_Year_Olds { get; set; }

        public int Number_of_5_Year_Olds { get; set; }

        public int Number_of_Kindergartners { get; set; }

        public int Number_of_1st_Graders { get; set; }

        public int Number_of_2nd_Graders { get; set; }

        public int Number_of_3rd_Graders { get; set; }

        public int Number_of_4th_Graders { get; set; }

        public int Number_of_5th_Graders { get; set; }

        public int Number_of_6th_Graders { get; set; }

        public int Number_of_7th_Graders { get; set; }

        public int Number_of_8th_Graders { get; set; }

        public int Number_of_9th_Graders { get; set; }

        public int Number_of_10th_Graders { get; set; }

        public int Number_of_11th_Graders { get; set; }

        public int Number_of_12th_Graders { get; set; }

        public int Number_of_Graduated_Seniors { get; set; }
    }
}