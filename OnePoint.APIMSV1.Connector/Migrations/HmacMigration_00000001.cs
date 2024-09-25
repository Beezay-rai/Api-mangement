using FluentMigrator;
using OnePoint.PDK.Migrations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnePoint.APIMSV1.Connector.Migrations
{
    [Migration(000000001, "HMAC INIT")]
    public class HmacMigration_00000001 : CustomMigrations
    {
        public override void Down()
        {
            Delete.Table("HmacAuthentication");
        }

        public override void Up()
        {
            Create.Table("HmacAuthentication")
              .WithColumn("Id").AsInt32().NotNullable().PrimaryKey().Identity()
              .WithColumn("ClientId").AsString(50).NotNullable()
              .WithColumn("ClientSecret").AsString(50).NotNullable();
        }
    }


}
