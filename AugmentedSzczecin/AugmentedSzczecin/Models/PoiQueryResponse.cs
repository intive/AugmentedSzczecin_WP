using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AugmentedSzczecin.Models
{
    public static class PoiQueryResponse
    {
        private static List<dynamic> listOfFunctions = new List<dynamic>
            {
                new Func<string, IList<JToken>> (ParsePoiPlacesQuery),
                new Func<string, IList<JToken>> (ParsePoiEventsQuery),
                new Func<string, IList<JToken>> (ParsePoiPeopleQuery),
                new Func<string, IList<JToken>> (ParsePoiCommercialQuery),
            };

        public static IList<JToken> ParsePoiPlacesQuery(string json)
        {
            IList<JToken> placesResults = null;
            if (JObject.Parse(json)["places"] != null)
            {
                placesResults = JObject.Parse(json)["places"].Children().ToList();
            }
            return placesResults;
        }

        public static IList<JToken> ParsePoiEventsQuery(string json)
        {
            IList<JToken> eventsResults = null;
            if (JObject.Parse(json)["events"] != null)
            {
                eventsResults = JObject.Parse(json)["events"].Children().ToList();
            }
            return eventsResults;
        }

        public static IList<JToken> ParsePoiPeopleQuery(string json)
        {
            IList<JToken> peopleResults = null;
            if (JObject.Parse(json)["people"] != null)
            {
                peopleResults = JObject.Parse(json)["people"].Children().ToList();
            }
            return peopleResults;
        }

        public static IList<JToken> ParsePoiCommercialQuery(string json)
        {
            IList<JToken> commercialResults = null;
            if(JObject.Parse(json)["commercial"] != null)
            {
                commercialResults = JObject.Parse(json)["commercial"].Children().ToList();
            }
            return commercialResults;
        }

        public static IList<JToken> ParsePoiAllQuery(string json)
        {
            IList<JToken> results = null;

            IList<JToken> temporaryResults = null;

            for (int i = 0; i < listOfFunctions.Capacity; i++)
            {
                temporaryResults = listOfFunctions[i](json);
                if (temporaryResults != null)
                {
                    if (results == null)
                    {
                        results = temporaryResults;
                    }
                    else
                    {
                        results = results.Concat(temporaryResults).ToList();
                    }
                }
            }

            return results;
        }
    }
}
