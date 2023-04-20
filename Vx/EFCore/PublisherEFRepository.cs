﻿using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Vx.Models;
using Vx.Repositories;

namespace Vx.EFCore
{
    public class PublisherEFRepository <TContext>: IPublisherRepository where TContext: VxDbContext
    {
        private readonly TContext _dbContext;


        public PublisherEFRepository(TContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Publisher?> GetByIdAsync(string id)
        {
            return await _dbContext.Publishers
                .Where(s => s.Id == id)
                .FirstOrDefaultAsync();
        }

        public async Task SaveAsync(Publisher publisher)
        {
            await _dbContext.AddAsync(publisher);
            await _dbContext.SaveChangesAsync();
        }

        public async Task UpdateAsync(Publisher publisher)
        {
            _dbContext.Update(publisher);
            await _dbContext.SaveChangesAsync();
        }

        public async Task Delete(Publisher publisher)
        {
            _dbContext.Remove(publisher);
            await _dbContext.SaveChangesAsync();
        }
    }
}