﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teste_PAD
{
    public class Event_Going
    {
        public int Id { get; set; }
        public int EventId { get; set; }
        public string Username { get; set; }
    }
}