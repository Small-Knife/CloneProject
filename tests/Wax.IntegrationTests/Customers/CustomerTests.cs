﻿using System;
using System.Threading.Tasks;
using Mediator.Net;
using Shouldly;
using Wax.Core.Exceptions;
using Wax.Core.Repositories;
using Wax.Messages.Commands.Customers;
using Xunit;

namespace Wax.IntegrationTests.Customers;

public class CustomerTests : IntegrationTestBase
{
    public CustomerTests(IntegrationFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task ShouldCreateNewCustomer()
    {
        await Run<IMediator, ICustomerRepository>(async (mediator, repo) =>
        {
            var createCustomerCommand = new CreateCustomerCommand
            {
                Name = "Microsoft",
                Address = "Microsoft Corporation One Microsoft Way Redmond, WA 98052-6399 USA",
                Contact = "(800) 426-9400"
            };

            var createCustomerResponse = await mediator.SendAsync<CreateCustomerCommand, CreateCustomerResponse>(
                createCustomerCommand);

            var customer = await repo.GetByIdAsync(createCustomerResponse.CustomerId);

            customer.Name.ShouldBe(createCustomerCommand.Name);
            customer.Address.ShouldBe(createCustomerCommand.Address);
        });
    }

    [Fact]
    public async Task ShouldUpdateCustomer()
    {
        var customerId = await CreateDefaultCustomer();

        await Run<IMediator, ICustomerRepository>(async (mediator, repo) =>
        {
            var updateCustomerCommand = new UpdateCustomerCommand
            {
                CustomerId = customerId,
                Name = "Google",
                Address = "111 8th Ave, New York, NY 10011, United States",
                Contact = "+12125650000"
            };

            await mediator.SendAsync(updateCustomerCommand);

            var customer = await repo.GetByIdAsync(customerId);

            customer.Name.ShouldBe(updateCustomerCommand.Name);
            customer.Address.ShouldBe(updateCustomerCommand.Address);
        });
    }

    [Fact]
    public async Task ShouldDeleteCustomer()
    {
        var customerId = await CreateDefaultCustomer();

        await Run<IMediator, ICustomerRepository>(async (mediator, repo) =>
        {
            await mediator.SendAsync(new DeleteCustomerCommand
            {
                CustomerId = customerId
            });

            await Should.ThrowAsync<EntityNotFoundException>(async () =>
                await repo.GetByIdAsync(customerId));
        });
    }

    private async Task<Guid> CreateDefaultCustomer()
    {
        return await Run<IMediator, Guid>(async mediator =>
        {
            var createCustomerResponse = await mediator.SendAsync<CreateCustomerCommand, CreateCustomerResponse>(
                new CreateCustomerCommand
                {
                    Name = "Microsoft",
                    Address = "Microsoft Corporation One Microsoft Way Redmond, WA 98052-6399 USA",
                    Contact = "(800) 426-9400"
                });

            return createCustomerResponse.CustomerId;
        });
    }
}