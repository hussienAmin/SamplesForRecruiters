using KafApp.Config;
using KafApp.Models;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Net.Http;
using System.Security.Cryptography;
using System.Security.Policy;


namespace KafApp.Repo.Server
{

    public partial class ServerRepo : Irepo
    {
        public ServerRepo()
        {
            service = new(new HttpClient());

        }
        HttpClientService service;

        public async Task<KafQueryResult> GetAllPersons<T>() where T : KafEntity
        {
             return await service.GetAsync($"{typeof(T).Name}/GetAllPersons");
        }
}