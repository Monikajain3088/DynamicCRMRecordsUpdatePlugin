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
                        // get the Account latest number from counter entity record
                        Entity CounterEntity = GetLatestAccountNumber(service);
                        int sequenceNumber = (int)CounterEntity["rel_sequencenumber"];
                        int incrementVal = (int)CounterEntity["rel_increment"];
                        if (sequenceNumber == 0)
                        {
                            int totAccount = GetTotalAccountCount(service);
                            tracingService.Trace("accountPlugin: {0}", totAccount.ToString());
                            entity.Attributes["rel_totalaccounts"] = totAccount + incrementVal; // assign value in total account counts 
                            CounterEntity["rel_sequencenumber"] = totAccount + incrementVal;
                            tracingService.Trace("Account number updated when sequence is 0");
                        }
                        else
                        {
                            tracingService.Trace("accountPlugin: {0}", sequenceNumber.ToString());
                            entity.Attributes["rel_totalaccounts"] = sequenceNumber + incrementVal;
                            CounterEntity["rel_sequencenumber"] = sequenceNumber + incrementVal; ;
                            tracingService.Trace("Account number updated when sequence is greater than 0");
                        }
                        service.Update(CounterEntity);

                    }
                    catch (Exception ex)
                    {
                        tracingService.Trace("Plugin Exception: " + ex);
                        throw new InvalidPluginExecutionException(ex.ToString());
                    }
                }
                else
                    return;
            }

        }
        /// <summary>
        /// Get Total Account records
        /// </summary>
        /// <param name="service"></param>
        /// <returns>Total count of existing account</returns>
        private int GetTotalAccountCount(IOrganizationService service)
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
            return totAccount;
        }

        /// <summary>
        /// get the lastest account number from Counter entity 
        /// </summary>
        /// <param name="organizationService"></param>
        /// <returns>Counter entity</returns>
        private Entity GetLatestAccountNumber(IOrganizationService organizationService)
        {
            string _lastnumberStr = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                                      <entity name='rel_counter'>
                                        <attribute name='rel_counterid' />
                                        <attribute name='rel_name' />
                                        <attribute name='rel_sequencenumber' />
                                        <attribute name='rel_increment' />
                                        <order attribute='rel_name' descending='false' />
                                       <filter type='and'>
                                       <condition attribute='rel_name' operator='eq' value='Account' />
                                       </filter>
                                      </entity>
                                    </fetch>";

            EntityCollection AccountCounterCol = (EntityCollection)organizationService.RetrieveMultiple(new FetchExpression(_lastnumberStr));
            Entity counterAccount = AccountCounterCol[0];
            return counterAccount;
        }
    }
}
