﻿using System;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using Wilson.Accounting.Data;
using Wilson.Scheduler.Core.Entities;
using Wilson.Web.Events.Interfaces;
using System.Collections.Generic;

namespace Wilson.Web.Events.Handlers
{
    public class PaycheckCreatedHandler : Handler
    {
        public PaycheckCreatedHandler(IServiceProvider serviceProvider) 
            : base(serviceProvider)
        {
        }

        public override async Task Handle(IDomainEvent args)
        {
            var eventArgs = args as PaycheckCreated;
            if (eventArgs == null)
            {
                throw new InvalidCastException();
            }

            var accDbContext = this.ServiceProvider.GetService<AccountingDbContext>();

            Mapper.Initialize(cfg =>
            {
                cfg.CreateMap<Paycheck, Accounting.Core.Entities.Paycheck>().ForMember(x => x.Employee, opt => opt.Ignore());
            });

            if (eventArgs.Paychecks != null)
            {
                var accPaychecks = 
                    Mapper.Map<IEnumerable<Paycheck>, IEnumerable<Accounting.Core.Entities.Paycheck>>(eventArgs.Paychecks);

                await accDbContext.Set<Accounting.Core.Entities.Paycheck>().AddRangeAsync(accPaychecks);
            }

            if (eventArgs.Paycheck != null)
            {
                var accPaycheck = Mapper.Map<Paycheck, Accounting.Core.Entities.Paycheck>(eventArgs.Paycheck);
                await accDbContext.Set<Accounting.Core.Entities.Paycheck>().AddAsync(accPaycheck);
            }

            await accDbContext.SaveChangesAsync();
        }
    }
}