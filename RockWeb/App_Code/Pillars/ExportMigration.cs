// <copyright>
// Copyright Pillars Inc.
// </copyright>
//

using System;
using System.Text;

using Rock.Data;

namespace RockWeb.Pillars
{
    public class ExportMigration : IMigration
    {
        private StringBuilder _generatedSql = new StringBuilder();

        /// <summary>
        /// Gets the migration helper.
        /// </summary>
        /// <value>
        /// The migration helper.
        /// </value>
        public Rock.Data.MigrationHelper Helper
        {
            get
            {
                if ( _helper == null )
                {
                    _helper = new Rock.Data.MigrationHelper( this );
                }
                return _helper;
            }
        }
        private Rock.Data.MigrationHelper _helper = null;

        public void Sql( string sql )
        {
            _generatedSql.AppendLine( "EXEC sp_executesql N'" );
            _generatedSql.Append( sql.Replace( "'", "''" ) );
            _generatedSql.AppendLine( "'" );
        }

        public string GeneratedSql
        {
            get
            {
                var sb = new StringBuilder();
                sb.AppendLine( "BEGIN TRANSACTION" );
                sb.AppendLine( "BEGIN TRY" );
                sb.Append( _generatedSql.ToString() );
                sb.AppendLine( "END TRY" );
                sb.AppendLine( "BEGIN CATCH" );
                sb.AppendLine( "ROLLBACK TRANSACTION" );
                sb.AppendLine( "END CATCH" );
                sb.AppendLine( "IF @@TRANCOUNT > 0" );
                sb.AppendLine( "COMMIT TRANSACTION" );

                return sb.ToString();
            }
        }
    }
}