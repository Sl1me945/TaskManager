﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskManager.Models.Enums;

namespace TaskManager.Interfaces
{
    public interface ITask
    {
        Guid Id { get; init; }
        string Title { get; set; }
        string Description { get; set; }
        DateTime CreatedAt { get; init; }
        DateTime DueDate { get; set; }
        bool IsCompleted { get; }
        Priority Priority { get; set; }

        void MarkAsCompleted ();
    }
}
