﻿namespace ExamBook.Entities
{
    public class TestSpeciality:Entity
    {
        public ExaminationSpeciality? ExaminationSpeciality { get; set; }
        public ulong? ExaminationSpecialityId { get; set; }

        public Test Test { get; set; } = null!;
        public ulong TestId { get; set; }
        
    }
}