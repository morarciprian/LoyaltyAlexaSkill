using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Alexa.NET;
using Alexa.NET.Request;
using Alexa.NET.Request.Type;
using Alexa.NET.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ServiceReference;

namespace LoyaltyAlexaSkill.Controllers
{
   // [Authorize]
    [Produces("application/json")]
    [Route("api/Alexa")]
    public class AlexaController : Controller
    {
        [HttpPost]
        public async Task<dynamic> Post([FromBody]SkillRequest input)
        {
            var speech = new Alexa.NET.Response.SsmlOutputSpeech();
            var finalResponse = new SkillResponse();
            // check what type of a request it is like an IntentRequest or a LaunchRequest
            var requestType = input.GetRequestType();

            if (requestType == typeof(IntentRequest))
            {
                // do some intent-based stuff
                var intentRequest = input.Request as IntentRequest;

                // check the name to determine what you should do
                if (intentRequest.Intent.Name.Equals("GetPoints"))
                {
                    long? points = 0;
                    var client = new CustomApiClient();
                    var result = await client.QueryPointsAsync("API", "EXTERNAL", 1, "921722255", ParamType.Msisdn, 1, null, null, null, null, false, null, null, null);
                    if (result != null)
                    {
                        foreach (var wallet in result.Wallets)
                        {
                            points = points + wallet.Points;
                        }
                    }
                    // create the speech response - cards still need a voice response
                    speech.Ssml = $"<speak>You currently have {points} loyalty points available in your account.</speak>";
                    // create the card response
                    finalResponse = ResponseBuilder.TellWithCard(speech, "GetPoints", $"You currently have {points} loyalty points available in your account.");
                }
                // check the name to determine what you should do
                if (intentRequest.Intent.Name.Equals("GetItems"))
                {
                    string items = string.Empty;
                    var client = new CustomApiClient();
                    var result = await client.QueryAvailableItemsAsync("API", "EXTERNAL", 1, "921722255", ParamType.Msisdn, 1, null, null, null, null, null);
                    if (result != null)
                    {
                        items = string.Join(", ", result.Items.Select(z => z.Name));
                    }
                    // create the speech response - cards still need a voice response
                    speech.Ssml = $"<speak>Here is the list of items available for you: {items}.</speak>";
                    // create the card response
                    //finalResponse = ResponseBuilder.TellWithCard(speech, "GetItems", $"Here is the list of items available for you: {items}.");

                    // create the speech reprompt
                    var repromptMessage = new PlainTextOutputSpeech();
                    repromptMessage.Text = "Would you like to add any of these to your shoping cart?";

                    // create the reprompt
                    var repromptBody = new Alexa.NET.Response.Reprompt();
                    repromptBody.OutputSpeech = repromptMessage;

                    // create the response
                    finalResponse = ResponseBuilder.AskWithCard(speech, "GetItems", $"Here is the list of items available for you: {items}.", repromptBody);

                }
                // check the name to determine what you should do
                if (intentRequest.Intent.Name.Equals("AddToBasket"))
                {
                    // create the speech response - cards still need a voice response
                    speech.Ssml = "<speak>Your item was successfully added to shopping cart.</speak>";
                    // create the card response
                    finalResponse = ResponseBuilder.TellWithCard(speech, "AddToBasket", "Your item was successfully added to shopping cart.");
                }
            }
            else if (requestType == typeof(Alexa.NET.Request.Type.LaunchRequest))
            {
                // default launch path executed
            }
            else if (requestType == typeof(AudioPlayerRequest))
            {
                // do some audio response stuff
            }

            return finalResponse;

        }
    }
}