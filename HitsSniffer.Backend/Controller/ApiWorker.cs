﻿using System;
using HitsSniffer.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using uzLib.Lite.Extensions;

namespace HitsSniffer.Controller
{
    public static class ApiWorker
    {
        public static string ApiUrl => "https://api.github.com/{0}/{1}";
        public static string ReposHandle => "repos";
        public static string ApiReposTemplate => string.Format(ApiUrl, ReposHandle, "{0}");

        /// <summary>
        /// Determines whether [is organization or user owner].
        /// </summary>
        /// <param name="data">The data.</param>
        /// <returns>
        ///   <c>true</c> if [is true the owner is an user] [the specified data]; otherwise, <c>false</c>.
        /// </returns>
        public static bool? IsOrgOrUserOwner(this RepoData data)
        {
            try
            {
                string url = string.Format(ApiReposTemplate, data);
                var jObj = JsonConvert.DeserializeObject<JObject>(url.MakeRequest());

                return jObj["owner"]["type"].ToObject<string>().ToLowerInvariant() == "user";
            }
            catch
            {
                // Couldn't find repo
                return null;
            }
        }

        /// <summary>
        /// Gets the type from owner.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <returns></returns>
        public static Type GetTypeFromOwner(this RepoData data)
        {
            var value = IsOrgOrUserOwner(data);
            if (!value.HasValue)
                return null;

            return value.HasValue && value.Value
                ? typeof(UserData)
                : typeof(OrgData);
        }
    }
}