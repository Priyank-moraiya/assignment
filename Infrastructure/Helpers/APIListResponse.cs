﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Helpers
{
    public class APIListResponse<T>
    {
        public int Page {  get; set; }
        public int TotalPages { get; set; }
        public List<T>? Data { get; set; }
    }
}
