global using Xunit;
global using FluentAssertions;
global using NSubstitute;

global using Identity.Domain.AggregatesModel.UsersAggregate;
global using Identity.Domain.Events;
global using Identity.Domain.Exceptions;
global using Identity.Domain.Services;

global using Identity.Infrastructure;
global using Identity.Infrastructure.Services;
global using Identity.Infrastructure.Idempotency;
global using Identity.Infrastructure.IntegrationEvents;
global using Identity.Infrastructure.Repositories;

global using Identity.API.Application.Commands.RegisterUser;
global using Identity.API.Application.Commands.Login;
global using Identity.API.Application.Commands.ChangePassword;
global using Identity.API.Application.Commands.RefreshToken;
global using Identity.API.Application.Commands.Identified;
global using Identity.API.Application.Behaviors;
global using Identity.API.Application.Validations;
global using Identity.API.Application.Models;
global using Identity.API.Application.IntegrationEvents;
global using Identity.API.Application.IntegrationEvents.EventHandling;
global using Identity.API.Application.IntegrationEvents.Events;

global using EventBus.Abstractions;
global using EventBus.Events;
global using Identity.API.Application.Queries.GetUserById;
global using Identity.API.Application.Queries.GetAllUsers;
global using Identity.API.Application.DomainEventHandlers;
global using Identity.API.Middleware;

global using Mediator;
global using Mediator.Commands;
global using Mediator.Notifications;
global using Mediator.Behaviors;

global using Core.SeedWork;

global using Microsoft.EntityFrameworkCore;
global using Microsoft.EntityFrameworkCore.Storage;
global using Microsoft.Extensions.Logging;
global using Microsoft.Extensions.Logging.Abstractions;
global using Microsoft.Extensions.Options;
global using Microsoft.AspNetCore.Http;
