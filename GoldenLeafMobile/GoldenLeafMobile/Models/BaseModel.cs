﻿using Newtonsoft.Json;
using SQLite;

namespace GoldenLeafMobile.Models
{
    public abstract class BaseModel
    {
        [JsonProperty("id")]
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public bool Syncronized { get; set; }

       
    }
}
