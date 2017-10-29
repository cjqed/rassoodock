﻿using System;
using System.Text.RegularExpressions;
using AutoMapper;
using Rassoodock.Databases;
using Rassoodock.SqlServer.Windows.Models.SqlModels;

namespace Rassoodock.SqlServer.Windows.Mappings
{
    public class RoutineSqlModelTypeConverter : ITypeConverter<RoutinesSqlModel, StoredProcedure>
    {
        public StoredProcedure Convert(RoutinesSqlModel source, StoredProcedure destination, ResolutionContext context)
        {
            /* In SqlServer, the routine definition needs to like this:
             *  SET QUOTED_IDENTIFIER ON/OFF
             *  GO
             *  SET ANSI_NULLS ON/OFF
             *  GO
             *  comments
             *  CREATE PROC[EDURE]
             *  NAME
             *  ...
             *  
             *  The name is whatever is defined by the user, so maybe brackets,
             *  maybe not... maybe schema, maybe not. 
             *  But we need to standardize on always brackets, always add schema
             *
             * NAME could be any of the following formats:
             * [sch].[na me]
             * sch.[na me]
             * sch.name
             * [sch].name
             * name
             * [na me]
             */

            var regex = new Regex($@"\[?({Regex.Escape(source.Specific_Schema)})?\]?\.?\[?{Regex.Escape(source.Specific_Name)}\]?");

            var newName = $"[{source.Specific_Schema}].[{source.Specific_Name}]";

            var text = regex.Replace(source.definition, newName, 1);

            var textEndsWithNewLine = text.EndsWith(Environment.NewLine);

            text = $"SET QUOTED_IDENTIFIER {OnOrOff(source.uses_quoted_identifier)}" + Environment.NewLine +
                   "GO" + Environment.NewLine +
                   $"SET ANSI_NULLS {OnOrOff(source.uses_ansi_nulls)}" + Environment.NewLine +
                   "GO" + Environment.NewLine +
                   text + (textEndsWithNewLine ? string.Empty : Environment.NewLine) +
                   "GO" + Environment.NewLine;

            return new StoredProcedure
            {
                Name = source.Specific_Name,
                Schema = source.Specific_Schema,
                Text = text
            };
        }

        private string OnOrOff(bool b) => b ? "ON" : "OFF";
        
    }
}
