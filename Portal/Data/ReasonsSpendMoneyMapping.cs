﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;

namespace Portal.Data
{
    public partial class ReasonsSpendMoneyMapping
    {
        public long Id { get; set; }
        public long ReasonsId { get; set; }
        public long SpendMoneyId { get; set; }

        public virtual MosbReasons Reasons { get; set; }
        public virtual MosbSpendMoney SpendMoney { get; set; }
    }
}