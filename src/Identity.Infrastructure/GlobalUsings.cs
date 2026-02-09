global using System.Data;
global using System.IdentityModel.Tokens.Jwt;
global using System.Security.Claims;
global using System.Security.Cryptography;
global using System.Text;

global using Identity.Domain.AggregatesModel.UsersAggregate;
global using Identity.Domain.Exceptions;
global using Identity.Domain.Services;

global using Identity.Infrastructure.Idempotency;
global using Identity.Infrastructure.IntegrationEvents;
global using Identity.Infrastructure.Repositories;
global using Identity.Infrastructure.Services;

global using Mediator;

global using Core.SeedWork;

global using FluentValidation;

global using Microsoft.AspNetCore.Authentication.JwtBearer;
global using Microsoft.AspNetCore.Http;
global using Microsoft.EntityFrameworkCore;
global using Microsoft.EntityFrameworkCore.Metadata.Builders;
global using Microsoft.EntityFrameworkCore.Migrations;
global using Microsoft.EntityFrameworkCore.Storage;
global using Microsoft.Extensions.Configuration;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Options;
global using Microsoft.IdentityModel.Tokens;
