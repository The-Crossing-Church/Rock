<%@ WebHandler Language="C#" Class="RockWeb.Webhooks.ProtectMyMinistryV2" %>
// 
// Copyright (C) Protect My Ministry - All Rights Reserved.
// 
using System;
using System.Collections.Generic;
using System.IO;
using System.Web;
using System.Linq;
using System.Xml.Linq;

using Rock;
using Rock.Web.Cache;
using Rock.Model;

namespace RockWeb.Webhooks
{
    /// <summary>
    /// Handles the background check results sent from Protect My Ministry
    /// </summary>
    public class ProtectMyMinistryV2 : IHttpHandler
    {
        public void ProcessRequest( HttpContext context )
        {
            HttpRequest request = context.Request;
            HttpResponse response = context.Response;

            response.ContentType = "text/plain";

            if ( request.HttpMethod != "POST" )
            {
                response.Write( "Invalid Request Type." );
                return;
            }

            try
            {
                string postedData = string.Empty;
                using ( var reader = new StreamReader( request.InputStream ) )
                {
                    postedData = reader.ReadToEnd();
                }

                if ( postedData.IsNullOrWhiteSpace() || !postedData.Contains( "<SSO" ) )
                {
                    response.Write( "Invalid Request Content." );
                }

                XDocument xResult = XDocument.Parse( postedData );

                // Get the orderid from the XML
                string orderId = string.Empty;
                foreach ( var xReferenceNumberElement in xResult.Descendants( "ReferenceNumber" ) )
                {
                    orderId = xReferenceNumberElement.Value;
                }

                if ( orderId.IsNotNullOrWhiteSpace() )
                {
                    var rockContext = new Rock.Data.RockContext();

                    // Find and update the associated workflow
                    var workflowService = new WorkflowService( rockContext );
                    var initialWorkflow = new WorkflowService( rockContext ).Get( orderId.AsInteger() );
                    Workflow workflow = null;

                    if ( initialWorkflow != null )
                    {
                        initialWorkflow.LoadAttributes();

                        var reviewActivityGuid = com.protectmyministry.SystemGuid.WorkflowActivityType.PROTECT_MY_MINISTRY_V2_REVIEW.AsGuid();
                        if ( initialWorkflow.IsActive && !initialWorkflow.ActiveActivities.Any( a => a.ActivityType.Guid == reviewActivityGuid ) )
                        {
                            workflow = initialWorkflow;
                        }
                        else
                        {
                            var updateWorkflowType = WorkflowTypeCache.Get( com.protectmyministry.SystemGuid.WorkflowType.PROTECT_MY_MINISTRY_UPDATE_V2.AsGuid() );
                            if ( updateWorkflowType != null )
                            {
                                workflow = Rock.Model.Workflow.Activate( updateWorkflowType, initialWorkflow.Name, rockContext );
                                foreach ( var attr in workflow.Attributes )
                                {
                                    workflow.SetAttributeValue( attr.Key, initialWorkflow.GetAttributeValue( attr.Key ) );
                                }
                                workflowService.Add( workflow );
                                rockContext.WrapTransaction( () =>
                                {
                                    rockContext.SaveChanges();
                                    workflow.SaveAttributeValues( rockContext );
                                } );
                            }
                        }

                        if ( workflow != null )
                        {
                            com.protectmyministry.BackgroundCheck.ProtectMyMinistryV2.SaveResults( xResult, workflow, initialWorkflow.Id, rockContext );

                            rockContext.WrapTransaction( () =>
                            {
                                rockContext.SaveChanges();
                                workflow.SaveAttributeValues( rockContext );
                                foreach ( var activity in workflow.Activities )
                                {
                                    activity.SaveAttributeValues( rockContext );
                                }
                            } );

                            try
                            {
                                List<string> workflowErrors;
                                workflowService.Process( workflow, out workflowErrors );
                            }
                            catch ( Exception ex )
                            {
                                ExceptionLogService.LogException( ex );
                            }
                        }
                    }
                }

                try
                {
                    // Return the success XML to PMM
                    XDocument xdocResult = new XDocument( new XDeclaration( "1.0", "UTF-8", "yes" ),
                        new XElement( "OrderXML",
                            new XElement( "Success", "TRUE" ) ) );

                    response.StatusCode = 200;
                    response.ContentType = "text/xml";
                    response.AddHeader( "Content-Type", "text/xml" );
                    xdocResult.Save( response.OutputStream );
                }
                catch { }
            }
            catch ( SystemException ex )
            {
                ExceptionLogService.LogException( ex, context );
            }
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }

    }
}