namespace Identity.Infrastructure.EntityConfigurations;

public sealed class ClientRequestConfiguration 
    : IEntityTypeConfiguration<ClientRequest>
{
    public void Configure(EntityTypeBuilder<ClientRequest> builder)
    {
        builder.ToTable("requests");
    }
}