﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ExamBook.Entities;
using ExamBook.Exceptions;
using ExamBook.Identity.Models;
using ExamBook.Models;
using ExamBook.Models.Data;
using ExamBook.Utils;
using Microsoft.EntityFrameworkCore;
using Vx.Models;
using Vx.Services;

namespace ExamBook.Services
{
    public class StudentService
    {
        private readonly DbContext _dbContext;
        private readonly EventService _eventService;
        private readonly PublisherService _publisherService;
        private readonly StudentSpecialityService _studentSpecialityService;

        public StudentService(DbContext dbContext, 
            EventService eventService, 
            PublisherService publisherService, 
            StudentSpecialityService studentSpecialityService)
        {
            _dbContext = dbContext;
            _eventService = eventService;
            _publisherService = publisherService;
            _studentSpecialityService = studentSpecialityService;
        }


        public async Task<ActionResultModel<Student>> AddAsync(Space space, StudentAddModel model, User user)
        {
            Asserts.NotNull(space, nameof(space));
            Asserts.NotNull(model, nameof(model));
            Asserts.NotNull(user, nameof(user));

            
            if (await ContainsAsync(space, model.RId))
            {
                throw new UsedValueException("StudentCodeUsed");
            }
            
            string normalizedRid = model.RId.Normalize().ToUpper();
            var specialities = _dbContext.Set<Speciality>()
                .Where(e => model.SpecialityIds.Contains(e.Id))
                .ToList();
            
            var publisher = await _publisherService.AddAsync();
            
            Student student = new()
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                BirthDate = model.BirthDate,
                Sex = model.Sex,
                NormalizedRId = normalizedRid,
                RId = model.RId,
                Space = space,
                PublisherId = publisher.Id
            };
            student.Specialities = (await _studentSpecialityService.CreateSpecialitiesAsync(student, specialities)).ToList();
            
            await _dbContext.AddAsync(student);
            await _dbContext.SaveChangesAsync();

            var publisherIds = new List<string> {publisher.Id, space.PublisherId};
            publisherIds.AddRange(specialities.Select(s => s.PublisherId));
            
            var @event = await _eventService.EmitAsync(publisherIds, user.ActorId, "STUDENT_ADD", student);
            
            return new ActionResultModel<Student>(student, @event);
        }

        

        public async Task<Event> ChangeRId(Student student, string rId, User user)
        {
            Asserts.NotNull(user, nameof(user));
            Asserts.NotNull(student, nameof(student));
            Asserts.NotNull(student.Space, nameof(student.Space));
            
            if (await ContainsAsync(student.Space, rId))
            {
                throw new UsedValueException("StudentRIdUsed");
            }

            var data = new ChangeValueData<string>(student.RId, rId);
            string normalizedRid = rId.Normalize().ToUpper();
            student.RId = rId;
            student.NormalizedRId = normalizedRid;
            _dbContext.Update(student);
            await _dbContext.SaveChangesAsync();

            var publisherIds = new List<string> { student.PublisherId, student.Space.PublisherId };
            return await _eventService.EmitAsync(publisherIds, user.ActorId, "STUDENT_CHANGE_RID", data);
        }


        public async Task ChangeInfo(Student student, StudentChangeInfoModel model)
        {
            Asserts.NotNull(student, nameof(student));
            Asserts.NotNull(student.Space, nameof(student.Space));
            Asserts.NotNull(model, nameof(model));

            student.Sex = model.Sex;
            student.BirthDate = model.BirthDate;
            student.FirstName = model.FirstName;
            student.LastName = model.LastName;
            _dbContext.Update(student);
            await _dbContext.SaveChangesAsync();
        }
        
        


        public async Task<bool> ContainsAsync(Space space, string rId)
        {
            Asserts.NotNull(space, nameof(space));
            Asserts.NotNullOrWhiteSpace(rId, nameof(rId));

            string normalized = rId.Normalize().ToUpper();
            return await _dbContext.Set<Student>()
                .AnyAsync(s => space.Id == s.SpaceId && s.RId == normalized && s.DeletedAt == null);
        }
        
        
        
        
        

        public async Task<Student?> FindAsync(Space space, string rId)
        {
            Asserts.NotNull(space, nameof(space));
            Asserts.NotNullOrWhiteSpace(rId, nameof(rId));

            string normalized = rId.Normalize().ToUpper();
            var student = await _dbContext.Set<Student>()
                .FirstOrDefaultAsync(s => space.Id == s.SpaceId && s.RId == normalized);

            if (student == null)
            {
                throw new ElementNotFoundException("StudentNotFoundByRId");
            }

            return student;
        }

        
        


        public async Task MarkAsDeleted(Student student)
        {
            Asserts.NotNull(student, nameof(student));
            student.Sex = '0';
            student.BirthDate = DateTime.MinValue;
            student.FirstName = "";
            student.LastName = "";
            student.RId = "";
            student.NormalizedRId = "";
            student.DeletedAt = DateTime.Now;
            _dbContext.Update(student);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<Event> DeleteAsync(Student student, User user)
        {
            Asserts.NotNull(student, nameof(student));
            Asserts.NotNull(student.Space, nameof(student.Space));
            Asserts.NotNull(user, nameof(user));
           // var studentSpecialities = await _dbContext.Set<StudentSpeciality>()
           //      .Where(p => student.Equals(p.StudentId))
           //      .ToListAsync();

           student.FirstName = "";
           student.LastName = "";
           student.Sex = '0';
           student.BirthDate = DateTime.MinValue;
           student.RId = "";

           _dbContext.Update(student);
           await _dbContext.SaveChangesAsync();
           
           var publisherIds = new List<string> {
               student.PublisherId, 
               student.Space.PublisherId
           };

           return await _eventService.EmitAsync(publisherIds, user.ActorId, "STUDENT_DELETE", student);
        }
    }
}