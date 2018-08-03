using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using Audit.Core;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Audit.NET.DynamoDB.Providers
{
    public class DynamoDBDataProvider : AuditDataProvider
    {
        public override object InsertEvent(AuditEvent auditEvent)
        {
            var client = GetClient(auditEvent);
            var doc = Document.FromJson(JsonConvert.SerializeObject(auditEvent));
            Table eventTable = Table.LoadTable(client, "Event");
            var result = eventTable.UpdateItemAsync(doc);

            result.Wait();
            return result.Result;
        }

        private IAmazonDynamoDB GetClient(AuditEvent auditEvent)
        {
            var client = new AmazonDynamoDBClient();

            return client;
        }

        private Table GetTable(IAmazonDynamoDB client)
        {
            Table table;
           if(Table.TryLoadTable(client, "Event", out table))
            {
                return table;
            }
           else
            {
                CreateTableRequest request = new CreateTableRequest();
                client.CreateTableAsync(request);
            }

        }
    }
}
