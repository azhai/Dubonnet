using System;
using System.Collections.Generic;
using Dubonnet.QueryBuilder.Compilers;

namespace Dubonnet.QueryBuilder
{
    public partial class QueryFactory<Q>
    {
        /// <summary>
        /// Compile sql and params.
        /// </summary>
        public (string, Dictionary<string, object>) CompileSql(string sql, object[] args)
        {
            if (instance._compiler == null)
            {
                instance._compiler = GetCompiler(instance.EngineScope);
            }
            var res = _compiler.Compile(sql, args);
            instance.Log(res.ToString());
            return (res.Sql, res.NamedBindings);
        }
        
        /// <summary>
        /// Compile itself.
        /// </summary>
        public (string, Dictionary<string, object>) CompileSql()
        {
            if (instance._compiler == null)
            {
                instance._compiler = GetCompiler(instance.EngineScope);
            }
            var res = _compiler.Compile(instance);
            instance.Log(res.ToString());
            return (res.Sql, res.NamedBindings);
        }
        
        public static Compiler<Q> GetCompiler(string engine)
        {
            switch (engine)
            {
                case "firebird":
                    return new FirebirdCompiler<Q>();
                case "mysql":
                    return new MySqlCompiler<Q>();
                case "oracle":
                case "oracle11g":
                    return new Oracle11gCompiler<Q>();
                case "pgsql":
                case "postgres":
                    return new PostgresCompiler<Q>();
                default:
                    return new SqlServerCompiler<Q>();
            }
        }
        
        public Q For(string engine, Func<Q, Q> fn)
        {
            instance.EngineScope = engine;
            var result = fn.Invoke(instance);
            // reset the engine
            instance.EngineScope = null;
            return result;
        }
        
        public Q ForFirebird(Func<Q, Q> fn)
        {
            return For("firebird", fn);
        }
        
        public Q ForMySql(Func<Q, Q> fn)
        {
            return For("mysql", fn);
        }
        
        public Q ForOracle11g(Func<Q, Q> fn)
        {
            return For("oracle11g", fn);
        }
        
        public Q ForPostgres(Func<Q, Q> fn)
        {
            return For("postgres", fn);
        }
        
        public Q ForSqlServer(Func<Q, Q> fn)
        {
            return For("sqlsrv", fn);
        }
    }
}