﻿using System.Threading.Tasks;
using ExamBook.Entities;
using ExamBook.Models;
using Microsoft.EntityFrameworkCore;

namespace ExamBook.Services
{
    public class MemberService
    {
        private readonly DbContext _dbContext;
        
        public async Task<Member> AddMember(Space space, MemberAddModel model)
        {
            var user = await _dbContext.Set<User>().FindAsync(model.UserId);
            if (await IsSpaceMember(space, user))
            {
                
            }

            Member member = await CreateMember(space, model);
            await _dbContext.AddAsync(member);
            await _dbContext.SaveChangesAsync();

            return member;
        }

        public async Task<Member> CreateMember(Space space, MemberAddModel addModel)
        {
            var user = await _dbContext.Set<User>().FindAsync(addModel.UserId);
            var member = new Member
            {
                UserId = user!.Id,
                Space = space,
                IsAdmin = addModel.IsAdmin
            };

            return member;
        }


        public async Task<bool> IsSpaceMember(Space space, User user)
        {
            return await _dbContext.Set<Member>()
                .AnyAsync(m => m.SpaceId == space.Id && m.UserId == user.Id);
        }
    }
}