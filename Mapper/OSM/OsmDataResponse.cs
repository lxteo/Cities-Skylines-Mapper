using System;
using System.Xml.Serialization;

namespace Mapper.OSM
{
    /// <remarks/>
    [XmlType(AnonymousType = true)]
    [XmlRoot(Namespace = "", IsNullable = false)]
    public class OsmDataResponse
    {
        private string noteField;
        private bool uploadField;
        private osmMeta metaField;
        private osmBounds boundsField;
        private osmNode[] nodeField;
        private osmWay[] wayField;
        private osmRelation[] relationField;
        private decimal versionField;
        private string generatorField;

        /// <upload/>
        public bool upload
        {
            get { return uploadField; }
            set { uploadField = value; }
        }


        /// <remarks/>
        public string note
        {
            get { return noteField; }
            set { noteField = value; }
        }

        /// <remarks/>
        public osmMeta meta
        {
            get { return metaField; }
            set { metaField = value; }
        }

        /// <remarks/>
        public osmBounds bounds
        {
            get { return boundsField; }
            set { boundsField = value; }
        }

        /// <remarks/>
        [XmlElement("node")]
        public osmNode[] node
        {
            get { return nodeField; }
            set { nodeField = value; }
        }

        /// <remarks/>
        [XmlElement("way")]
        public osmWay[] way
        {
            get { return wayField; }
            set { wayField = value; }
        }

        /// <remarks/>
        [XmlElement("relation")]
        public osmRelation[] relation
        {
            get { return relationField; }
            set { relationField = value; }
        }

        /// <remarks/>
        [XmlAttribute]
        public decimal version
        {
            get { return versionField; }
            set { versionField = value; }
        }

        /// <remarks/>
        [XmlAttribute]
        public string generator
        {
            get { return generatorField; }
            set { generatorField = value; }
        }
    }

    /// <remarks/>
    [XmlType(AnonymousType = true)]
    public class osmMeta
    {
        private DateTime osm_baseField;

        /// <remarks/>
        [XmlAttribute]
        public DateTime osm_base
        {
            get { return osm_baseField; }
            set { osm_baseField = value; }
        }
    }

    /// <remarks/>
    [XmlType(AnonymousType = true)]
    public class osmBounds
    {
        private decimal minlatField;

        private decimal minlonField;

        private decimal maxlatField;

        private decimal maxlonField;

        /// <remarks/>
        [XmlAttribute]
        public decimal minlat
        {
            get { return minlatField; }
            set { minlatField = value; }
        }

        /// <remarks/>
        [XmlAttribute]
        public decimal minlon
        {
            get { return minlonField; }
            set { minlonField = value; }
        }

        /// <remarks/>
        [XmlAttribute]
        public decimal maxlat
        {
            get { return maxlatField; }
            set { maxlatField = value; }
        }

        /// <remarks/>
        [XmlAttribute]
        public decimal maxlon
        {
            get { return maxlonField; }
            set { maxlonField = value; }
        }
    }

    /// <remarks/>
    [XmlType(AnonymousType = true)]
    public class osmNode
    {
        private osmNodeTag[] tagField;

        private string idField;

        private decimal latField;

        private decimal lonField;

        private int versionField;

        private DateTime timestampField;

        private uint changesetField;

        private uint uidField;

        private string userField;

        /// <remarks/>
        [XmlElement("tag")]
        public osmNodeTag[] tag
        {
            get { return tagField; }
            set { tagField = value; }
        }

        /// <remarks/>
        [XmlAttribute]
        public string id
        {
            get { return idField; }
            set { idField = value; }
        }

        /// <remarks/>
        [XmlAttribute]
        public decimal lat
        {
            get { return latField; }
            set { latField = value; }
        }

        /// <remarks/>
        [XmlAttribute]
        public decimal lon
        {
            get { return lonField; }
            set { lonField = value; }
        }

        /// <remarks/>
        [XmlAttribute]
        public int version
        {
            get { return versionField; }
            set { versionField = value; }
        }

        /// <remarks/>
        [XmlAttribute]
        public DateTime timestamp
        {
            get { return timestampField; }
            set { timestampField = value; }
        }

        /// <remarks/>
        [XmlAttribute]
        public uint changeset
        {
            get { return changesetField; }
            set { changesetField = value; }
        }

        /// <remarks/>
        [XmlAttribute]
        public uint uid
        {
            get { return uidField; }
            set { uidField = value; }
        }

        /// <remarks/>
        [XmlAttribute]
        public string user
        {
            get { return userField; }
            set { userField = value; }
        }
    }

    /// <remarks/>
    [XmlType(AnonymousType = true)]
    public class osmNodeTag
    {
        private string kField;

        private string vField;

        /// <remarks/>
        [XmlAttribute]
        public string k
        {
            get { return kField; }
            set { kField = value; }
        }

        /// <remarks/>
        [XmlAttribute]
        public string v
        {
            get { return vField; }
            set { vField = value; }
        }
    }

    /// <remarks/>
    [XmlType(AnonymousType = true)]
    public class osmWay
    {
        private osmWayND[] ndField;

        private osmWayTag[] tagField;

        private uint idField;

        private int versionField;

        private DateTime timestampField;

        private uint changesetField;

        private uint uidField;

        private string userField;

        /// <remarks/>
        [XmlElement("nd")]
        public osmWayND[] nd
        {
            get { return ndField; }
            set { ndField = value; }
        }

        /// <remarks/>
        [XmlElement("tag")]
        public osmWayTag[] tag
        {
            get { return tagField; }
            set { tagField = value; }
        }

        /// <remarks/>
        [XmlAttribute]
        public uint id
        {
            get { return idField; }
            set { idField = value; }
        }

        /// <remarks/>
        [XmlAttribute]
        public int version
        {
            get { return versionField; }
            set { versionField = value; }
        }

        /// <remarks/>
        [XmlAttribute]
        public DateTime timestamp
        {
            get { return timestampField; }
            set { timestampField = value; }
        }

        /// <remarks/>
        [XmlAttribute]
        public uint changeset
        {
            get { return changesetField; }
            set { changesetField = value; }
        }

        /// <remarks/>
        [XmlAttribute]
        public uint uid
        {
            get { return uidField; }
            set { uidField = value; }
        }

        /// <remarks/>
        [XmlAttribute]
        public string user
        {
            get { return userField; }
            set { userField = value; }
        }
    }

    /// <remarks/>
    [XmlType(AnonymousType = true)]
    public class osmWayND
    {
        private string refField;

        /// <remarks/>
        [XmlAttribute]
        public string @ref
        {
            get { return refField; }
            set { refField = value; }
        }
    }

    /// <remarks/>
    [XmlType(AnonymousType = true)]
    public class osmWayTag
    {
        private string kField;

        private string vField;

        /// <remarks/>
        [XmlAttribute]
        public string k
        {
            get { return kField; }
            set { kField = value; }
        }

        /// <remarks/>
        [XmlAttribute]
        public string v
        {
            get { return vField; }
            set { vField = value; }
        }
    }

    /// <remarks/>
    [XmlType(AnonymousType = true)]
    public class osmRelation
    {
        private osmRelationMember[] memberField;

        private osmRelationTag[] tagField;

        private uint idField;

        private int versionField;

        private DateTime timestampField;

        private uint changesetField;

        private uint uidField;

        private string userField;

        /// <remarks/>
        [XmlElement("member")]
        public osmRelationMember[] member
        {
            get { return memberField; }
            set { memberField = value; }
        }

        /// <remarks/>
        [XmlElement("tag")]
        public osmRelationTag[] tag
        {
            get { return tagField; }
            set { tagField = value; }
        }

        /// <remarks/>
        [XmlAttribute]
        public uint id
        {
            get { return idField; }
            set { idField = value; }
        }

        /// <remarks/>
        [XmlAttribute]
        public int version
        {
            get { return versionField; }
            set { versionField = value; }
        }

        /// <remarks/>
        [XmlAttribute]
        public DateTime timestamp
        {
            get { return timestampField; }
            set { timestampField = value; }
        }

        /// <remarks/>
        [XmlAttribute]
        public uint changeset
        {
            get { return changesetField; }
            set { changesetField = value; }
        }

        /// <remarks/>
        [XmlAttribute]
        public uint uid
        {
            get { return uidField; }
            set { uidField = value; }
        }

        /// <remarks/>
        [XmlAttribute]
        public string user
        {
            get { return userField; }
            set { userField = value; }
        }
    }

    /// <remarks/>
    [XmlType(AnonymousType = true)]
    public class osmRelationMember
    {
        private string typeField;

        private string refField;

        private string roleField;

        /// <remarks/>
        [XmlAttribute]
        public string type
        {
            get { return typeField; }
            set { typeField = value; }
        }

        /// <remarks/>
        [XmlAttribute]
        public string @ref
        {
            get { return refField; }
            set { refField = value; }
        }

        /// <remarks/>
        [XmlAttribute]
        public string role
        {
            get { return roleField; }
            set { roleField = value; }
        }
    }

    /// <remarks/>
    [XmlType(AnonymousType = true)]
    public class osmRelationTag
    {
        private string kField;

        private string vField;

        /// <remarks/>
        [XmlAttribute]
        public string k
        {
            get { return kField; }
            set { kField = value; }
        }

        /// <remarks/>
        [XmlAttribute]
        public string v
        {
            get { return vField; }
            set { vField = value; }
        }
    }
}