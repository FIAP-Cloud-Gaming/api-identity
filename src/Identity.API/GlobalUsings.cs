global using System.Reflection;

global using Identity.Domain.AggregatesModel.UsersAggregate;
global using Identity.Domain.Events;
global using Identity.Domain.Exceptions;
global using Identity.Domain.Services;

global using Identity.Infrastructure;
global using Identity.Infrastructure.Idempotency;

global using Identity.API.Application.Behaviors;
global using Identity.API.Application.IntegrationEvents;
global using Identity.API.Application.Models;

global using Mediator;
global using Mediator.Behaviors;
global using Mediator.Commands;
global using Mediator.Extensions;
global using Mediator.Notifications;
global using Mediator.Queries;

global using Core.SeedWork;

global using FluentValidation;

global using Microsoft.AspNetCore.Mvc;
global using Microsoft.EntityFrameworkCore;
global using Microsoft.Extensions.Diagnostics.HealthChecks;
global using Microsoft.AspNetCore.Authentication;
global using Microsoft.AspNetCore.Authentication.JwtBearer;
global using Microsoft.AspNetCore.OpenApi;
global using Microsoft.OpenApi;

global using ILogger = Microsoft.Extensions.Logging.ILogger;

global using Serilog;
global using Serilog.Sinks.Grafana.Loki;

global using Scalar.AspNetCore;