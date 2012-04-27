<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:mods="http://www.loc.gov/mods/v3"
    xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance"
    xmlns:xlink="http://www.w3.org/1999/xlink"
    xmlns:ms="urn:schemas-microsoft-com:xslt"
    version="1.0">
    
    <!-- Transform ContentDM African Maps record to MODS -->
    
    <xsl:param name="current-date" select="''"/>
    <xsl:param name="class-auth" select="''"/>
    <xsl:param name="related-source-file" select="''"/>
    <xsl:param name="source-file" select="''"/>
    <xsl:param name="dts" select="''"/>

  <xsl:output indent="yes" omit-xml-declaration="yes" encoding="UTF-8"/>
    
    <xsl:template match="record">
        <xsl:element name="mods:mods">
            <xsl:attribute name="version">3.4</xsl:attribute>
            <xsl:attribute name="xsi:schemaLocation">http://www.loc.gov/mods/v3 http://www.loc.gov/standards/mods/mods.xsd</xsl:attribute>
            
            <xsl:element name="mods:typeOfResource">cartographic</xsl:element>

            <xsl:element name="mods:titleInfo">
                <xsl:element name="mods:title">
                    <xsl:value-of select="title"/>
                </xsl:element>
            </xsl:element>
            
            <xsl:element name="mods:name">
                <xsl:element name="mods:displayForm">
                    <xsl:value-of select="creator"/>
                </xsl:element>
                <xsl:element name="mods:role">
                    <xsl:element name="mods:roleTerm">
                        <xsl:attribute name="authority">marcrelator</xsl:attribute>
                        <xsl:attribute name="type">text</xsl:attribute>
                        <xsl:text>Creator</xsl:text>
                    </xsl:element>
                    <xsl:element name="mods:roleTerm">
                        <xsl:attribute name="authority">marcrelator</xsl:attribute>
                        <xsl:attribute name="type">code</xsl:attribute>
                        <xsl:text>cre</xsl:text>
                    </xsl:element>
                </xsl:element>
            </xsl:element>
            
            <xsl:element name="mods:originInfo">
                <xsl:element name="mods:place">
                    <xsl:element name="mods:placeTerm">
                        <xsl:value-of select="place_of_publication"/>
                    </xsl:element>
                </xsl:element>
                <xsl:element name="mods:dateCreated">
                    <xsl:value-of select="date"/>
                </xsl:element>
            </xsl:element>
            
            <xsl:call-template name="SplitRegions" >
                <xsl:with-param name="str" select="region"/>
            </xsl:call-template>
            
            <xsl:for-each select="subject">
                <xsl:element name="mods:subject">
                    <xsl:attribute name="authority">lcsh</xsl:attribute>
                    <xsl:element name="mods:topic">
                        <xsl:value-of select="."/>
                    </xsl:element>
                </xsl:element>
            </xsl:for-each>
            
            <xsl:for-each select="keyword">
                <xsl:element name="mods:subject">
                    <xsl:element name="mods:topic">
                        <xsl:value-of select="."/>
                    </xsl:element>
                </xsl:element>
            </xsl:for-each>
            
            <xsl:element name="mods:subject">
                <xsl:element name="mods:cartographics">
                    <xsl:element name="mods:scale">
                        <xsl:value-of select="scale"/>
                    </xsl:element>
                </xsl:element>
            </xsl:element>

            <xsl:element name="mods:genre">
                <xsl:value-of select="genre"/>
            </xsl:element>
            
            <xsl:element name="mods:physicalDescription">
                <xsl:element name="mods:form">
                    <xsl:attribute name="authority">marccategory</xsl:attribute>
                    <xsl:text>map</xsl:text>
                </xsl:element>
                <xsl:element name="mods:form">
                    <xsl:attribute name="type">technique</xsl:attribute>
                    <xsl:value-of select="technique"/>
                </xsl:element>
                <xsl:element name="mods:extent">
                    <xsl:value-of select="dimensions"/>
                </xsl:element>
                <xsl:element name="mods:note">
                    <xsl:text>Color: </xsl:text><xsl:value-of select="color"/>
                </xsl:element>
                <xsl:element name="mods:form">
                    <xsl:attribute name="authority">marccategory</xsl:attribute>
                    <xsl:text>electronic resource</xsl:text>
                </xsl:element>
                <xsl:element name="mods:internetMediaType">
                    <xsl:value-of select="format"/>
                </xsl:element>
            </xsl:element>
            
            <xsl:element name="mods:language">
                <xsl:element name="mods:languageTerm">
                    <xsl:attribute name="type">text</xsl:attribute>
                    <xsl:value-of select="language"/>
                </xsl:element>
            </xsl:element>
            
            <xsl:element name="mods:relatedItem">
                <xsl:attribute name="type">host</xsl:attribute>
                <xsl:attribute name="displayLabel">Source</xsl:attribute>
                <xsl:if test="string-length(normalize-space($related-source-file)) > 0">
                    <xsl:attribute name="xlink:href"><xsl:value-of select="$related-source-file"/></xsl:attribute>
                </xsl:if>
                <xsl:element name="mods:titleInfo">
                    <xsl:element name="mods:title">
                        <xsl:value-of select="source"/>
                    </xsl:element>
                </xsl:element>
                <xsl:element name="mods:location">
                    <xsl:element name="mods:physicalLocation">
                        <xsl:text>University of Illinois at Urbana-Champaign</xsl:text>
                    </xsl:element>
                    <xsl:element name="mods:physicalLocation">
                        <xsl:attribute name="authority">oclcorg</xsl:attribute>
                        <xsl:text>UIU</xsl:text>
                    </xsl:element>
                    <xsl:element name="mods:physicalLocation">
                        <xsl:attribute name="authority">marcorg</xsl:attribute>
                        <xsl:text>IU</xsl:text>
                    </xsl:element>
                    <xsl:element name="mods:holdingSimple">
                        <xsl:element name="mods:copyInformation">
                            <xsl:element name="mods:subLocation"><xsl:value-of select="holding_library"/></xsl:element>
                            <xsl:element name="mods:shelfLocator"><xsl:value-of select="local_call_number"/></xsl:element>
                        </xsl:element>
                    </xsl:element>
                </xsl:element>
            </xsl:element>
            
            <xsl:element name="mods:relatedItem">
                <xsl:attribute name="type">host</xsl:attribute>
                <xsl:attribute name="displayLabel">ContentDM Collection</xsl:attribute>
                <xsl:element name="mods:titleInfo">
                    <xsl:element name="mods:title">
                        <xsl:value-of select="collection"/>
                    </xsl:element>
                </xsl:element>
            </xsl:element>

            
            <xsl:element name="mods:location">
                <xsl:element name="mods:url">
                  <xsl:attribute name="access">raw object</xsl:attribute>
                  <xsl:attribute name="note">Original URL</xsl:attribute>
                  <xsl:attribute name="dateLastAccessed">
                    <xsl:value-of select="$current-date"/>
                  </xsl:attribute>
                  <xsl:value-of select="jpeg2000_url"/>
                </xsl:element>
            </xsl:element>

            <xsl:element name="mods:location">
                <xsl:element name="mods:url">
                    <xsl:attribute name="access">object in context</xsl:attribute>
                    <xsl:attribute name="usage">primary</xsl:attribute>
                  <xsl:attribute name="dateLastAccessed">
                    <xsl:value-of select="$current-date"/>
                  </xsl:attribute>
                  <xsl:value-of select="item_url"/>
                </xsl:element>
            </xsl:element>
            

            <xsl:if test="string-length(normalize-space(record/notes)) > 0">
                <xsl:element name="mods:note">
                    <xsl:value-of select="notes"/>
                </xsl:element>
            </xsl:if>
            
            <xsl:element name="mods:classification">
                <xsl:if test="string-length(normalize-space($class-auth)) > 0">
                <xsl:attribute name="authority"><xsl:value-of select="$class-auth"/></xsl:attribute>
                </xsl:if>
                <xsl:value-of select="local_call_number"/>
            </xsl:element>
            
            <xsl:element name="mods:identifier">
                <xsl:attribute name="type">map_no_in_basset_bibliography</xsl:attribute>
                <xsl:value-of select="map_no_in_basset_bibliography"/>
            </xsl:element>
            
            <xsl:element name="mods:identifier">
                <xsl:attribute name="type">file_name</xsl:attribute>
                <xsl:value-of select="file_name"/>
            </xsl:element>
            
            <xsl:if test="string-length(normalize-space(record/oclc_number)) > 0">
                <xsl:element name="mods:identifier">
                    <xsl:attribute name="type">oclc</xsl:attribute>
                    <xsl:value-of select="oclc_number"/>
                </xsl:element>
            </xsl:if>

            <xsl:element name="mods:identifier">
                <xsl:attribute name="type">contentdm_number</xsl:attribute>
                <xsl:value-of select="contentdm_number"/>
            </xsl:element>
            <xsl:element name="mods:identifier">
                <xsl:attribute name="type">contentdm_file_name</xsl:attribute>
                <xsl:value-of select="contentdm_file_name"/>
            </xsl:element>
            <xsl:element name="mods:identifier">
                <xsl:attribute name="type">contentdm_file_path</xsl:attribute>
                <xsl:value-of select="contentdm_file_path"/>
            </xsl:element>

            <xsl:if test="string-length(normalize-space(record/rights)) > 0">
                <xsl:element name="mods:accessCondition">
                    <xsl:value-of select="rights"/>
                </xsl:element>
            </xsl:if>
            
            <xsl:element name="mods:recordInfo">
                <xsl:element name="mods:recordContentSource">
                    <xsl:text>University of Illinois at Urbana-Champaign</xsl:text>
                </xsl:element>
                <xsl:element name="mods:recordContentSource">
                    <xsl:attribute name="authority">oclcorg</xsl:attribute>
                    <xsl:text>UIU</xsl:text>
                </xsl:element>
                <xsl:element name="mods:recordContentSource">
                    <xsl:attribute name="authority">marcorg</xsl:attribute>
                    <xsl:text>IU</xsl:text>
                </xsl:element>
                <xsl:element name="mods:recordOrigin">
                  <xsl:text>XSLT was used to convert the original </xsl:text>
                  <xsl:value-of select="$source-file"/>
                  <xsl:text> file exported from ContentDM.</xsl:text>
                </xsl:element>
                <xsl:if test="string-length($current-date) > 0">
                    <xsl:element name="mods:recordCreationDate">
                        <xsl:attribute name="encoding">w3cdtf</xsl:attribute>
                        <xsl:value-of select="$current-date"/>
                    </xsl:element> 
                </xsl:if>                    
            </xsl:element>

        </xsl:element>
    </xsl:template>
    
    <xsl:template name="SplitRegions">
        <xsl:param name="str" select="."/>
        <xsl:variable name="before" select="normalize-space(substring-before($str,';'))"/>
        <xsl:variable name="after" select="normalize-space(substring-after($str,';'))"/>
        
        <xsl:choose>
            <xsl:when test="contains($str,';')">
                <xsl:element name="mods:subject">
                    <xsl:element name="mods:geographic">
                        <xsl:value-of select="normalize-space(substring-before($str,';'))"/>
                    </xsl:element>
                </xsl:element>
                <xsl:call-template name="SplitRegions" >
                    <xsl:with-param name="str" select="normalize-space(substring-after($str,';'))"/>
                </xsl:call-template>
            </xsl:when>
            <xsl:when test="string-length(normalize-space($str)) > 0">
                <xsl:element name="mods:subject">
                    <xsl:element name="mods:geographic">
                        <xsl:value-of select="normalize-space($str)"/>
                    </xsl:element>
                </xsl:element>
            </xsl:when>
        </xsl:choose>
        
    </xsl:template>
    
    
</xsl:stylesheet>