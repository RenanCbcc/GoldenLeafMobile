﻿using Golden_Leaf_Mobile.Models;
using GoldenLeafMobile.Models;
using GoldenLeafMobile.Service;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Extended;

namespace GoldenLeafMobile.ViewModels
{
    public class ListViewModel<T> : BaseViewModel where T : class
    {
        public readonly string FAIL = "OnFetchingEntities";
        public readonly string SELECTED = "OnEntitySelected";
        public Pagination<T> Pagination { get; set; }
        public InfiniteScrollCollection<T> Entities { get; set; }

        public List<string> Choises { get; set; }

        private string _searchBy;
        public string SearchBy
        {
            get { return _searchBy; }
            set { _searchBy = value; OnPropertyChanged(); }
        }


        private T _selectedEntity;
        public T SelectedEntity
        {
            get { return _selectedEntity; }
            set
            {
                _selectedEntity = value;
                MessagingCenter.Send(_selectedEntity, SELECTED);
            }
        }

        public ListViewModel()
        {
            Choises = new List<string>();
            Entities = new InfiniteScrollCollection<T>
            {
                OnLoadMore = async () =>
                {
                    await GetEntities();
                    return Pagination.Data;
                },
                OnCanLoadMore = () =>
                {
                    return Entities.Count < Pagination.Total;
                }
            };
        }

        public void AddChoises(params string[] list)
        {
            Choises.AddRange(list);
            SearchBy = Choises[0];
        }

        public async Task GetEntities(string queryParameter = "")
        {
            Wait = true;
            using (HttpClient httpClient = new HttpClient())
            {
                var api = new ApiService<T>(httpClient);
                var response = await api.GetEntitiesAsync(queryParameter);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadAsStringAsync();
                    Pagination = JsonConvert.DeserializeObject<Pagination<T>>(result);
                    if (!string.IsNullOrEmpty(queryParameter))
                    {
                        Entities.Clear();
                    }
                    Entities.AddRange(Pagination.Data);
                }
                else
                {
                    var content = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                    if (response.Content != null)
                        response.Content.Dispose();

                    MessagingCenter.Send(new SimpleHttpResponseException(response.StatusCode, response.ReasonPhrase, content),
                        FAIL);
                }

            }
            Wait = false;

        }


    }
}
