﻿using System.Collections.Generic;

namespace Rassoodock.Databases
{
    public class Database
    {
        public IEnumerable<StoredProcedure> StoredProcedures { get; set; }

        public IEnumerable<Table> Tables { get; set; }
    }
}
