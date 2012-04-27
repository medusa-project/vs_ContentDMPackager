<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:mods="http://www.loc.gov/mods/v3"
    xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance"
    xmlns:xlink="http://www.w3.org/1999/xlink"
    version="1.0">
    
    <!-- Transform ContentDM Historical Maps record to MODS -->
    
    
    <xsl:param name="current-date" select="''"/>
    <xsl:param name="class-auth" select="''"/>
    <xsl:param name="related-source-file" select="''"/>
    <xsl:param name="contentdm-collection" select="''"/>
    <xsl:param name="compound-object"/>
    <xsl:param name="link-compound-object" select="''"/>
    <xsl:param name="contentdm-number" select="''"/>
    <xsl:param name="source-file" select="''"/>


  <xsl:variable name="source" select="/"/>
    <xsl:variable name="record" select="//record[contentdm_number=$contentdm-number]"/>
    
    <xsl:output indent="yes" omit-xml-declaration="yes" encoding="UTF-8"/>
    
    <xsl:template match="record">
        <xsl:element name="mods:mods">
            <xsl:attribute name="version">3.4</xsl:attribute>
            <xsl:attribute name="ID">
                <xsl:text>pageptr_</xsl:text><xsl:value-of select="contentdm_number"/>
            </xsl:attribute>
            <xsl:attribute name="xsi:schemaLocation">http://www.loc.gov/mods/v3 http://www.loc.gov/standards/mods/mods.xsd</xsl:attribute>
            <xsl:apply-templates select="." mode="NO_WRAPPER">
                <xsl:with-param name="is-root" select="true()"/>
            </xsl:apply-templates>
        </xsl:element>
    </xsl:template>
    
    <xsl:template match="record" mode="NO_WRAPPER">
            <xsl:param name="is-root" select="false()"/>
            <xsl:param name="part-detail-title" select="''"/>
        
            <xsl:if test="$is-root">
                <xsl:element name="mods:typeOfResource">
                    <!--<xsl:if test="$compound-object">
                        <xsl:attribute name="collection">yes</xsl:attribute>
                    </xsl:if>-->
                    <xsl:text>cartographic</xsl:text>
                </xsl:element>
            </xsl:if>
        
            <xsl:if test="$is-root or normalize-space(title)!=normalize-space($record/title)">
            <xsl:if test="$part-detail-title!=normalize-space(title)">
            <xsl:element name="mods:titleInfo">
                <xsl:element name="mods:title">
                    <xsl:value-of select="title"/>
                </xsl:element>
            </xsl:element>
            </xsl:if>
            </xsl:if>
        
            <xsl:if test="$is-root or normalize-space(title_alternative)!=normalize-space($record/title_alternative)">
            <xsl:if test="string-length(normalize-space(title_alternative)) > 0">
            <xsl:if test="$part-detail-title!=normalize-space(title_alternative)">
               <xsl:element name="mods:titleInfo">
                   <xsl:attribute name="type">alternative</xsl:attribute>
                   <xsl:element name="mods:title">
                       <xsl:value-of select="title_alternative"/>
                   </xsl:element>
               </xsl:element>
            </xsl:if>
            </xsl:if>
            </xsl:if>
            
        <xsl:if test="$is-root or normalize-space(author)!=normalize-space($record/author)">
            <xsl:element name="mods:name">
                <xsl:element name="mods:displayForm">
                    <xsl:value-of select="author"/>
                </xsl:element>
                <xsl:element name="mods:role">
                    <xsl:element name="mods:roleTerm">
                        <xsl:attribute name="authority">marcrelator</xsl:attribute>
                        <xsl:attribute name="type">text</xsl:attribute>
                        <xsl:text>Author</xsl:text>
                    </xsl:element>
                    <xsl:element name="mods:roleTerm">
                        <xsl:attribute name="authority">marcrelator</xsl:attribute>
                        <xsl:attribute name="type">code</xsl:attribute>
                        <xsl:text>aut</xsl:text>
                    </xsl:element>
                </xsl:element>
            </xsl:element>
                </xsl:if>
            
        <xsl:if test="$is-root or normalize-space(contributors)!=normalize-space($record/contributors)">
            <xsl:if test="string-length(normalize-space(contributors)) > 0">
            <xsl:element name="mods:name">
                <xsl:element name="mods:displayForm">
                    <xsl:value-of select="contributors"/>
                </xsl:element>
                <xsl:element name="mods:role">
                    <xsl:element name="mods:roleTerm">
                        <xsl:attribute name="authority">marcrelator</xsl:attribute>
                        <xsl:attribute name="type">text</xsl:attribute>
                        <xsl:text>Contributor</xsl:text>
                    </xsl:element>
                    <xsl:element name="mods:roleTerm">
                        <xsl:attribute name="authority">marcrelator</xsl:attribute>
                        <xsl:attribute name="type">code</xsl:attribute>
                        <xsl:text>ctb</xsl:text>
                    </xsl:element>
                </xsl:element>
            </xsl:element>
            </xsl:if>
        </xsl:if>
        
        <xsl:if test="$is-root or normalize-space(concat(date,year))!=normalize-space(concat($record/date,$record/year))">
            <xsl:element name="mods:originInfo">
                <xsl:if test="$is-root or normalize-space(date)!=normalize-space($record/date)">
                    <xsl:if test="string-length(normalize-space(date)) > 0">
                    <xsl:element name="mods:dateCaptured">
                    <xsl:value-of select="date"/>
                    </xsl:element>
                    </xsl:if>
                </xsl:if>
                <xsl:if test="$is-root or normalize-space(year)!=normalize-space($record/year)">
                    <xsl:element name="mods:dateCreated">
                    <xsl:value-of select="year"/>
                    </xsl:element>
                    </xsl:if>
            </xsl:element>
        </xsl:if>
        
        <xsl:if test="$is-root or normalize-space(coverage_spatial)!=normalize-space($record/coverage_spatial)">
            <xsl:if test="string-length(normalize-space(coverage_spatial)) > 0">
            <xsl:element name="mods:subject">
                <xsl:element name="mods:geographic">
                    <xsl:value-of select="coverage_spatial"/>
                </xsl:element>
            </xsl:element>
            </xsl:if>
        </xsl:if>
        
            <xsl:for-each select="subject">
                <xsl:variable name="sbj" select="normalize-space(.)"/>
                <!-- TODO:  Subjects must match in same position ??? -->
                <xsl:if test="$is-root or not($record/subject[normalize-space(.)=$sbj])">
                    <xsl:element name="mods:subject">
                    <xsl:element name="mods:topic">
                        <xsl:value-of select="."/>
                    </xsl:element>
                    </xsl:element>
                    </xsl:if>
            </xsl:for-each>
            
        <xsl:if test="$is-root or normalize-space(scale)!=normalize-space($record/scale)">
            <xsl:if test="string-length(normalize-space(scale)) > 0 ">
            <xsl:element name="mods:subject">
                <xsl:element name="mods:cartographics">
                    <xsl:element name="mods:scale">
                        <xsl:value-of select="scale"/>
                    </xsl:element>
                </xsl:element>
            </xsl:element>
            </xsl:if>
        </xsl:if>
        
        <xsl:if test="$is-root or normalize-space(concat(format,description))!=normalize-space(concat($record/format,$record/description))">
            <xsl:element name="mods:physicalDescription">
                <xsl:if test="$is-root">
                <xsl:element name="mods:form">
                    <xsl:attribute name="authority">marccategory</xsl:attribute>
                    <xsl:text>map</xsl:text>
                </xsl:element>
                </xsl:if>
                <xsl:if test="$is-root or normalize-space(format)!=normalize-space($record/format)">
                    <xsl:element name="mods:form">
                    <xsl:attribute name="authority">marccategory</xsl:attribute>
                    <xsl:text>electronic resource</xsl:text>
                </xsl:element>
                <xsl:if test="string-length(normalize-space(format)) > 0">
                    <xsl:element name="mods:internetMediaType">
                        <xsl:value-of select="format"/>
                    </xsl:element>
                </xsl:if>
                </xsl:if>
                <xsl:if test="$is-root or normalize-space(description)!=normalize-space($record/description)">
                <xsl:if test="string-length(normalize-space(description)) > 0">
                <xsl:element name="mods:note">
                    <xsl:value-of select="description"/>
                </xsl:element>
                </xsl:if>
                    </xsl:if>
            </xsl:element>
        </xsl:if>
        
        <xsl:if test="$is-root or normalize-space(interpretation)!=normalize-space($record/interpretation)">
            <xsl:if test="string-length(normalize-space(interpretation)) > 0">
            <xsl:element name="mods:note">
                <xsl:attribute name="displayLabel">Interpretation</xsl:attribute>
                <xsl:value-of select="interpretation"/>
            </xsl:element>
            </xsl:if>
        </xsl:if>
        
        <xsl:if test="$is-root or normalize-space(language)!=normalize-space($record/language)">
            <xsl:if test="string-length(normalize-space(language)) > 0">
             <xsl:element name="mods:language">
                <xsl:element name="mods:languageTerm">
                    <xsl:attribute name="type">text</xsl:attribute>
                    <xsl:value-of select="language"/>
                </xsl:element>
            </xsl:element>
            </xsl:if>
        </xsl:if>
        
            <xsl:if test="$is-root">
            <xsl:element name="mods:relatedItem">
                <xsl:attribute name="type">host</xsl:attribute>
                <xsl:attribute name="displayLabel">Source</xsl:attribute>
                <xsl:if test="string-length(normalize-space($related-source-file)) > 0">
                    <xsl:attribute name="xlink:href"><xsl:value-of select="$related-source-file"/></xsl:attribute>
                </xsl:if>
                <xsl:if test="string-length(normalize-space(source)) > 0">
                <xsl:element name="mods:titleInfo">
                    <xsl:element name="mods:title">
                        <xsl:value-of select="source"/>
                    </xsl:element>
                </xsl:element>
                </xsl:if>
                <xsl:choose>
                    <xsl:when test="contains(collection,'Urbana')">
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
                                    <xsl:element name="mods:subLocation"><xsl:value-of select="collection"/></xsl:element>
                                    <xsl:element name="mods:shelfLocator"><xsl:value-of select="call_number"/></xsl:element>
                                </xsl:element>
                            </xsl:element>
                        </xsl:element>
                    </xsl:when>
                    <xsl:otherwise>
                        <xsl:element name="mods:physicalLocation"><xsl:value-of select="collection"/></xsl:element>
                        <xsl:element name="mods:shelfLocator"><xsl:value-of select="call_number"/></xsl:element>
                    </xsl:otherwise>
                </xsl:choose>
            </xsl:element>
        
            <xsl:if test="string-length(normalize-space($contentdm-collection)) > 0">
            <xsl:element name="mods:relatedItem">
                <xsl:attribute name="type">host</xsl:attribute>
                <xsl:attribute name="displayLabel">ContentDM Collection</xsl:attribute>
                <xsl:element name="mods:titleInfo">
                    <xsl:element name="mods:title">
                        <xsl:value-of select="$contentdm-collection"/>
                    </xsl:element>
                </xsl:element>
            </xsl:element>
            </xsl:if>
                
            </xsl:if>
        
            <xsl:if test="string-length(normalize-space(jpeg2000_url)) > 0">
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
            </xsl:if>
            
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
            
            <xsl:if test="string-length(normalize-space(view_mrsid_image_zoom)) > 0">
                <xsl:element name="mods:location">
                <xsl:element name="mods:url">
                    <xsl:attribute name="note">Link to Original MrSID Image viewer</xsl:attribute>
                  <xsl:attribute name="dateLastAccessed">
                    <xsl:value-of select="$current-date"/>
                  </xsl:attribute>
                  <xsl:value-of select="view_mrsid_image_zoom"/>
                </xsl:element>
            </xsl:element>
            </xsl:if>
            
        <xsl:if test="$is-root or normalize-space(call_number)!=normalize-space($record/call_number)">
            <xsl:element name="mods:classification">
                <xsl:if test="string-length(normalize-space($class-auth)) > 0">
                <xsl:attribute name="authority"><xsl:value-of select="$class-auth"/></xsl:attribute>
                </xsl:if>
                <xsl:value-of select="call_number"/>
            </xsl:element>
            </xsl:if>
            
            <xsl:if test="string-length(normalize-space(archival_file_name)) > 0">
            <xsl:element name="mods:identifier">
                <xsl:attribute name="type">archival_file_name</xsl:attribute>
                <xsl:value-of select="archival_file_name"/>
            </xsl:element>
            </xsl:if>
        
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

        <xsl:if test="$is-root or normalize-space(rights)!=normalize-space($record/rights)">
            <xsl:if test="string-length(normalize-space(record/rights)) > 0">
                <xsl:element name="mods:accessCondition">
                    <xsl:value-of select="rights"/>
                </xsl:element>
            </xsl:if>
        </xsl:if>
        
            <xsl:if test="$compound-object and substring(contentdm_file_path,string-length(contentdm_file_path)-3) = '.cpd'">
                <xsl:element name="mods:note">
                    <xsl:attribute name="displayLabel">ContentDM Compound Type</xsl:attribute>
                    <xsl:value-of select="$compound-object/cpd/type"/>
                </xsl:element>
                <xsl:apply-templates select="$compound-object"/>
            </xsl:if>
            
            <xsl:if test="$is-root">
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
            </xsl:if>
        
    </xsl:template>
    
    <xsl:template match="/cpd">
            <xsl:apply-templates select="node"/>
            <xsl:apply-templates select="page"/>
    </xsl:template>
    
    <xsl:template match="/cpd//node">
        <xsl:element name="mods:relatedItem">
            <xsl:attribute name="type">constituent</xsl:attribute>
            <xsl:element name="mods:part">
                <xsl:element name="mods:detail">
                    <xsl:element name="mods:title">
                        <xsl:value-of select="nodetitle"/>
                    </xsl:element>
                </xsl:element>
            </xsl:element>
            <xsl:apply-templates select="node"/>
            <xsl:apply-templates select="page"/>
        </xsl:element>
    </xsl:template>
    
    <xsl:template match="/cpd//page">
      <xsl:variable name="ptr" select="pageptr"/>
      <xsl:variable name="mfile" select="modsfile"/>
      <xsl:element name="mods:relatedItem">
            <xsl:attribute name="type">constituent</xsl:attribute>
            <xsl:attribute name="ID"><xsl:value-of select="concat('pageptr_',$ptr)"/></xsl:attribute>
            <xsl:if test="$link-compound-object='TRUE'">
              <!--<xsl:attribute name="xlink:href"><xsl:value-of select="concat('mods_',$ptr,'.xml')"/></xsl:attribute>-->
              <xsl:attribute name="xlink:href">
                <xsl:value-of select="$mfile"/>
              </xsl:attribute>
            </xsl:if>
            <xsl:element name="mods:part">
                <xsl:element name="mods:detail">
                    <xsl:element name="mods:title">
                        <xsl:value-of select="pagetitle"/>
                   </xsl:element>
                </xsl:element>
            </xsl:element>
            <xsl:if test="$link-compound-object!='TRUE'">
                <xsl:apply-templates select="$source/metadata/record[contentdm_number=$ptr]" mode="NO_WRAPPER">
                    <xsl:with-param name="part-detail-title" select="normalize-space(pagetitle)"/>
                </xsl:apply-templates>
            </xsl:if>
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