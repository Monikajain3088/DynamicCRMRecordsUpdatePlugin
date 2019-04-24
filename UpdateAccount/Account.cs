using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace UpdateAccount
{
    public class Account: IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {

            // Obtain the execution context from the service provider.
            IPluginExecutionContext context =
                (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));

            //Extract the tracing service for use in debugging sandboxed plug-ins.
            ITracingService tracingService =
                (ITracingService)serviceProvider.GetService(typeof(ITracingService));

            // Get a reference to the Organization service.
            IOrganizationServiceFactory factory =
                (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));

            IOrganizationService service = factory.CreateOrganizationService(context.UserId);

            if (context.Depth > 1) return;
            // The InputParameters collection contains all the data passed in the message request.
            if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
            {
                // Obtain the target entity from the input parameters.
                Entity entity = (Entity)context.InputParameters["Target"];

                if (entity.LogicalName == Constant.EntityName)
                {
                    try
                    {
                        // Get All existing Accounts
                        string _ExistingAccount = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                                              <entity name='account'>
                                                <attribute name='name' />
                                                <attribute name='accountid' />
                                                <attribute name='rel_totalaccounts' />
                                                <order attribute='name' descending='false' />
                                              </entity>
                                            </fetch>";

                        EntityCollection totExistingAccountCol = (EntityCollection)service.RetrieveMultiple(new FetchExpression(_ExistingAccount));
                        int totAccount = totExistingAccountCol.Entities.Count; // get existing account count
                        tracingService.Trace("accountPlugin: {0}", totAccount.ToString());
                        entity.Attributes["rel_totalaccounts"] = totAccount + Constant.increasedVal; // assign value in total account counts 
                        tracingService.Trace("Account Counts written");


                    }
                    catch (Exception e)
                    {

                        tracingService.Trace("accountPlugin: {0}", e.ToString());
                        throw;



                    }
                }
                else
                    return;

            }


        }
    }
}
