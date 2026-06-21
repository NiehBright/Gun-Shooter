using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    public partial class Currency
    {
        [SerializeField] GameObject model;
        public GameObject Model => model;

        private Pool pool;
        public Pool Pool => pool;

        partial void OnInitialised()
        {
            pool = new Pool(new PoolSettings(currencyType.ToString(), model, 1, true));
        }
    }
}