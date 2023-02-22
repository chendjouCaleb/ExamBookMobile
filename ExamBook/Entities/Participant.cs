﻿using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace ExamBook.Entities
{
    public class Participant:Entity
    {

        /// <summary>
        /// Registration id. Unique identifier of participant is real world.
        /// </summary>
        public string RId { get; set; } = "";

        public uint Index { get; set; }

        public string NormalizedRId { get; set; } = "";

        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";

        public string FullName => $"{FirstName} {LastName}";
        public DateTime BirthDate { get; set; }
        public char Sex { get; set; }

        [JsonIgnore] public Student? Student { get; set; }
        public ulong? StudentId { get; set; }

        [JsonIgnore]
        public Examination Examination { get; set; } = null!;
        public ulong ExaminationId { get; set; }

        [JsonIgnore] public List<ParticipantSpeciality> ParticipantSpecialities { get; set; } = new();

        [JsonIgnore] public List<Paper> Papers { get; set; } = new();
    }
}