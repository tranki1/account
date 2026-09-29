using Amazon.CDK;

namespace Account.Cdk;

public sealed class Program
{
    public static void Main(string[] args)
    {
        var app = new App();

        // eu-central-1 per the S1 spec (Mascus AWS account, Frankfurt).
        var env = new Amazon.CDK.Environment
        {
            Account = System.Environment.GetEnvironmentVariable("CDK_DEFAULT_ACCOUNT"),
            Region = System.Environment.GetEnvironmentVariable("CDK_DEFAULT_REGION") ?? "eu-central-1"
        };

        _ = new AccountServiceStack(app, "AccountServiceStack", new StackProps
        {
            Env = env,
            Description = "Accounts microservice: ECS Fargate + RDS PostgreSQL, Secrets Manager, SSM, AppConfig."
        });

        app.Synth();
    }
}
