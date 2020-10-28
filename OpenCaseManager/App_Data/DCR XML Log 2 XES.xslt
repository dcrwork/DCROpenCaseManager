<?xml version="1.0"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <xsl:template match="/">
    <log xmlns="http://www.xes-standard.org/" xes.version="2.0">
      <extension name="Concept" prefix="concept" uri="http://www.xes-standard.org/concept.xesext" />
      <extension name="Time" prefix="concept" uri="http://www.xes-standard.org/time.xesext" />
      <global scope="trace">
        <string key="concept:name" value="default"/>
        <string key="CASE_concept_instance" value="default"/>
      </global>
      <global scope="event">
        <string key="concept:name" value="default"/>
        <string key="lifecycle:transition" value="default"/>
        <string key="org:resource" value="default"/>
        <date key="time:timestamp" value="1970-01-01T00:00:00.000+00:00"/>
        <string key="concept:instance" value="default"/>
        <string key=".order" value="default"/>
      </global>
      <xsl:for-each select="log/trace">
        <xsl:if test="count(event)>0">
          <trace>
            <string key="concept:name">
              <xsl:attribute name="value">
                <xsl:choose>
                  <xsl:when test="@title=''">
                    <xsl:value-of select="@id"/>
                  </xsl:when>
                  <xsl:otherwise>
                    <xsl:value-of select="@title"/>
                  </xsl:otherwise>
                </xsl:choose>
              </xsl:attribute>
            </string>
            <date key="time:created">
              <xsl:attribute name="value">
                <xsl:value-of select="@created"/>
              </xsl:attribute>
            </date>
            <string key="CASE_concept_instance">
              <xsl:attribute name="value">
                <xsl:value-of select="@id"/>
              </xsl:attribute>
            </string>
            <xsl:variable name="pos">
              <xsl:value-of select="(position()-1)*100"/>
            </xsl:variable>
            <xsl:for-each select="event">
              <event>
                <string key="concept:name">
                  <xsl:attribute name="value">
                    <xsl:value-of select="@id"/>
                  </xsl:attribute>
                </string>
                <string key="lifecycle:transition" value="complete"/>
                <string key="org:resource">
                  <xsl:attribute name="value">
                    <xsl:value-of select="@role"/>
                  </xsl:attribute>
                </string>
                <date key="time:timestamp">
                  <xsl:attribute name="value">
                    <xsl:value-of select="@timestamp"/>
                  </xsl:attribute>
                </date>
                <string key="concept:instance">
                  <xsl:attribute name="value">
                    <xsl:value-of select="position()+$pos"/>
                  </xsl:attribute>
                </string>
                <string key=".order">
                  <xsl:attribute name="value">
                    <xsl:value-of select="position()+$pos"/>
                  </xsl:attribute>
                </string>
              </event>
            </xsl:for-each>
          </trace>
        </xsl:if>
      </xsl:for-each>
    </log>
  </xsl:template>
</xsl:stylesheet>