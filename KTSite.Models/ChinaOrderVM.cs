﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Text;

namespace KTSite.Models
{
    public class ChinaOrderVM
    {
        public ChinaOrder chinaOrder { get; set; }
        public IEnumerable<SelectListItem> ProductList { get; set; }
    }
}
