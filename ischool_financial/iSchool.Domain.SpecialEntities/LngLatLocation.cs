//using Microsoft.Data.SqlClient;
//using Microsoft.SqlServer.Types;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security;

namespace iSchool
{
    /// <summary>
    /// 经纬度
    /// </summary>
    public sealed partial class LngLatLocation
    {
        public LngLatLocation(double lng, double lat, int srid = 4326)
        {
            Lng = lng;
            Lat = lat;
            SRID = srid;
        }

        public double Lng { get; set; }
        public double Lat { get; set; }
        public int SRID { get; set; }

        public static void Init_With_Dapper()
        {            
            Dapper.SqlMapper.AddTypeHandler(typeof(LngLatLocation), new LngLatLocationTypeHandler1());

            ///
            /// 2020.07.29
            /// 经调试发现, sqlclient读取Udt类型的数据表字段时会强制?使用 Microsoft.SqlServer.Types.dll !!?, 如没找到此dll, 会报错.
            /// 但Microsoft.SqlServer.Types.dll没netcore版本,直接引用会使项目出现黄色警告线... 所以这里用动态加载.
            ///

            if (Type.GetType("Microsoft.SqlServer.Types.SqlGeography,Microsoft.SqlServer.Types") == null)
                Assembly.LoadFrom(Path.Combine(Path.GetDirectoryName(typeof(LngLatLocation).Assembly.ManifestModule.FullyQualifiedName), "Microsoft.SqlServer.Types.dll"));
        }
    }

    public class LngLatLocationTypeHandler1 : Dapper.SqlMapper.ITypeHandler
    {
        public void SetValue(IDbDataParameter parameter, object value)
        {
            /// dapper2.x 貌似使用 System.Data.SqlClient.dll
            if (parameter is SqlParameter sqlParameter)
            {
                sqlParameter.SqlDbType = SqlDbType.Udt;
                sqlParameter.UdtTypeName = "GEOGRAPHY";
                parameter.Value = value == null ? (object)DBNull.Value :
                    value is LngLatLocation location ? (new SqlServerBytesWriter { IsGeography = true }).Write(new Point(location.Lng, location.Lat) { SRID = location.SRID }) :
                    throw new NotSupportedException();
            }
        }

        public object Parse(Type destinationType, object value)
        {
            if (value == null || value is DBNull) return null;
            if (destinationType == typeof(LngLatLocation))
            {
                // select语句查询`(geography类型字段).Serialize()`
                if (value is byte[] bys)
                {
                    var p = (Point)(new SqlServerBytesReader { IsGeography = true }).Read(bys);
                    return new LngLatLocation(p.X, p.Y, p.SRID);
                }

                // select语句直接查询geography类型字段
                dynamic sqlGeography = value;
                return sqlGeography == null ? null : new LngLatLocation(sqlGeography.Long.Value, sqlGeography.Lat.Value, sqlGeography.STSrid.Value);
            }
            throw new NotSupportedException();
        }
    }

    public sealed partial class LngLatLocation
    {
        #region DistanceByLiejia
        /// <summary>
        /// 2个经纬度的距离 - 烈嘉算法
        /// </summary>
        /// <param name="other"></param>
        /// <returns>单位：米</returns>
        public double DistanceByLiejia(LngLatLocation other)
        {
            if (Equals(other, null) || this.SRID != other.SRID)
            {
                throw new InvalidOperationException("srid is not same");
            }
            if (this.SRID != 4326)
            {
                throw new InvalidOperationException("srid is not 4326");
            }

            double rad(double d)
            {
                return d * Math.PI / 180.0;
            }

            double radLat1 = rad(this.Lat);
            double radLat2 = rad(other.Lat);
            double a = radLat1 - radLat2;
            double b = rad(this.Lng) - rad(other.Lng);

            double s = 2 * Math.Asin(Math.Sqrt(Math.Pow(Math.Sin(a / 2), 2) + Math.Cos(radLat1) * Math.Cos(radLat2) * Math.Pow(Math.Sin(b / 2), 2)));
            s = s * SridList.GetEllipsoidParameters(this.SRID).semi_major;
            s = Math.Round(s * 10000) / 10000;
            return s;
        }
        #endregion DistanceByLiejia

        #region DotSpatial算法
        /// <summary>
        /// 2个经纬度的距离 - DotSpatial算法
        /// <br/>modify from https://github.com/ststeiger/DotSpatial
        /// <br/>较接近sqlserver的算法
        /// </summary>
        /// <returns>单位：米</returns>
        public static double DistanceBySpatial(double lng1, double lat1, double lng2, double lat2)
        {
            double a = 6378137;
            double b = 6356752.314; //6356752.31424518

            double goodAlpha = 0;
            double goodSigma = 0;
            double goodCos2SigmaM = 0;

            double lat_1 = lat1 * Math.PI / 180.0;
            double lon_1 = lng1 * Math.PI / 180.0;
            double lat_2 = lat2 * Math.PI / 180.0;
            double lon_2 = lng2 * Math.PI / 180.0;

            if (Math.Abs(Math.PI * 0.5 - Math.Abs(lat_1)) < 1E-10)
            {
                lat_1 = Math.Sign(lat_1) * (Math.PI * 0.5 - 1E-10);
            }
            if (Math.Abs(Math.PI * 0.5 - Math.Abs(lat_2)) < 1E-10)
            {
                lat_2 = Math.Sign(lat_2) * (Math.PI * 0.5 - 1E-10);
            }

            double f = (a - b) / a;

            double u1 = Math.Atan((1 - f) * Math.Tan(lat_1));
            double u2A = Math.Atan((1 - f) * Math.Tan(lat_2));

            lon_1 = lon_1 % (2 * Math.PI);
            lon_2 = lon_2 % (2 * Math.PI);

            double l = Math.Abs(lon_2 - lon_1);
            if (l > Math.PI)
            {
                l = 2.0 * Math.PI - l;
            }

            double lambda = l;
            int itercount = 0;
            bool notdone = true;

            while (notdone)
            {
                itercount++;
                if (itercount > 50)
                {
                    break;
                }

                double lambdaold = lambda;

                double sinsigma = Math.Sqrt(Math.Pow((Math.Cos(u2A) * Math.Sin(lambda)), 2)
                        + Math.Pow((Math.Cos(u1) * Math.Sin(u2A) - Math.Sin(u1) *
                        Math.Cos(u2A) * Math.Cos(lambda)), 2));

                double cossigma = Math.Sin(u1) * Math.Sin(u2A) +
                    Math.Cos(u1) * Math.Cos(u2A) * Math.Cos(lambda);

                double sigma = Math.Atan2(sinsigma, cossigma);

                double alpha = Math.Asin(Math.Cos(u1) * Math.Cos(u2A) *
                                         Math.Sin(lambda) / Math.Sin(sigma));

                double cos2SigmaM = Math.Cos(sigma) - 2.0 * Math.Sin(u1) *
                                    Math.Sin(u2A) / Math.Pow(Math.Cos(alpha), 2);

                double c = f / 16 * Math.Pow(Math.Cos(alpha), 2) * (4 + f * (4 - 3 *
                                                                             Math.Pow(Math.Cos(alpha), 2)));

                lambda = l + (1 - c) * f * Math.Sin(alpha)
                            * (sigma + c * Math.Sin(sigma) *
                            (cos2SigmaM + c * Math.Cos(sigma) *
                            (-1 + 2 * Math.Pow(cos2SigmaM, 2))));

                if (lambda > Math.PI)
                {
                    lambdaold = Math.PI;
                    lambda = Math.PI;
                }

                notdone = Math.Abs(lambda - lambdaold) > 1.0E-12;

                if (!double.IsNaN(alpha))
                {
                    goodAlpha = alpha;
                    goodSigma = sigma;
                    goodCos2SigmaM = cos2SigmaM;
                }

                System.Threading.Thread.Sleep(0);
            }

            double u2 = Math.Pow(Math.Cos(goodAlpha), 2) * (Math.Pow(a, 2) - Math.Pow(b, 2)) / Math.Pow(b, 2);
            double aa = 1 + u2 / 16384 * (4096 + u2 * (-768 + u2 * (320 - 175 * u2)));
            double bb = u2 / 1024 * (256 + u2 * (-128 + u2 * (74 - 47 * u2)));

            double deltasigma = bb * Math.Sin(goodSigma) * (goodCos2SigmaM + bb / 4 * (Math.Cos(goodSigma) * (-1 + 2 *
                Math.Pow(goodCos2SigmaM, 2)) - bb / 6 * goodCos2SigmaM * (-3 + 4 * Math.Pow(Math.Sin(goodSigma), 2)) * (-3 + 4 *
                Math.Pow(goodCos2SigmaM, 2))));

            double s = b * aa * (goodSigma - deltasigma);
            return s;
        }

        public double DistanceBySpatial(LngLatLocation other)
        {
            if (Equals(other, null) || this.SRID != other.SRID)
            {
                throw new InvalidOperationException("srid is not same");
            }
            if (this.SRID != 4326)
            {
                throw new InvalidOperationException("srid is not 4326");
            }
            return DistanceBySpatial(this.Lng, this.Lat, other.Lng, other.Lat);
        }
        #endregion DotSpatial算法
    }

    public sealed partial class LngLatLocation
    {
        /// <summary>
        /// 2个经纬度的距离 - sqlserver内部算法
        /// </summary>
        /// <param name="other"></param>
        /// <returns>单位：米</returns>
        [Obsolete("没找到支持linux的SqlServerSpatialXXX.dll")]
        public double DistanceBySql(LngLatLocation other)
        {
            if (Equals(other, null) || this.SRID != other.SRID)
            {
                throw new InvalidOperationException("srid is not same");
            }
            return GeodeticPointDistance(new Point(this.Lat, this.Lng), new Point(other.Lat, other.Lng), SridList.GetEllipsoidParameters(this.SRID));
        }

        [DllImport("SqlServerSpatial140.dll", CharSet = CharSet.None, ExactSpelling = false)]
        [SuppressUnmanagedCodeSecurity]
        static extern double GeodeticPointDistance(in Point p1, in Point p2, in EllipsoidParameters ep);

        [Serializable]
        struct Point
        {
            public double x;
            public double y;

            public Point(double X, double Y)
            {
                this.x = X;
                this.y = Y;
            }
        }

        struct EllipsoidParameters
        {
            public double semi_major;
            public double semi_minor;

            public EllipsoidParameters(double major, double minor)
            {
                this.semi_major = major;
                this.semi_minor = minor;
            }

            public double GetEccentricity()
            {
                return this.semi_minor / this.semi_major;
            }

            public double GetMaxCurvature()
            {
                double semiMinor = this.semi_minor;
                return this.semi_major / (semiMinor * semiMinor);
            }

            public double GetMinCurvature()
            {
                double semiMajor = this.semi_major;
                return this.semi_minor / (semiMajor * semiMajor);
            }
        }

        class SridInfo
        {
            public int spatial_reference_id;
            public string authority_name;
            public int authorized_spatial_reference_id;
            public string well_known_text;
            public string unit_of_measure;
            public double unit_conversion_factor;
            public double semi_major_axis;
            public double semi_minor_axis;

            public SridInfo(int spatial_reference_id, string authority_name, int authorized_spatial_reference_id, string well_known_text, string unit_of_measure, double unit_conversion_factor, double semi_major_axis, double semi_minor_axis)
            {
                this.spatial_reference_id = spatial_reference_id;
                this.authority_name = authority_name;
                this.authorized_spatial_reference_id = authorized_spatial_reference_id;
                this.well_known_text = well_known_text;
                this.unit_of_measure = unit_of_measure;
                this.unit_conversion_factor = unit_conversion_factor;
                this.semi_major_axis = semi_major_axis;
                this.semi_minor_axis = semi_minor_axis;
            }
        }

        class SridList
        {
            private static SortedList<int, SridInfo> _sridList;

            public static int Null => -1;

            static SridList()
            {
                _sridList = new SortedList<int, SridInfo>()
                {
                    { 4326, new SridInfo(4326, "EPSG", 4326, "GEOGCS[\"WGS 84\", DATUM[\"World Geodetic System 1984\", ELLIPSOID[\"WGS 84\", 6378137, 298.257223563]], PRIMEM[\"Greenwich\", 0], UNIT[\"Degree\", 0.0174532925199433]]", "metre", 1, 6378137, 6356752.314) },
                };
            }

            public SridList() { }

            public static EllipsoidParameters GetEllipsoidParameters(int srid)
            {
                var item = _sridList[srid];
                return new EllipsoidParameters(item.semi_major_axis, item.semi_minor_axis);
            }
        }
    }
}
