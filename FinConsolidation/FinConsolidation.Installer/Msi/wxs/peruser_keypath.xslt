<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0"
  xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
  xmlns:wix="http://wixtoolset.org/schemas/v4/wxs"
  exclude-result-prefixes="xsl wix">
  <xsl:output method="xml" indent="yes"/>

  <!-- identity -->
  <xsl:template match="@*|node()">
    <xsl:copy><xsl:apply-templates select="@*|node()"/></xsl:copy>
  </xsl:template>

  <!-- For each harvested Component:
       1) remove KeyPath="yes" from File (if present)
       2) inject a HKCU registry value as KeyPath -->
  <xsl:template match="wix:Component">
    <xsl:variable name="cmpId" select="@Id"/>
    <xsl:copy>
      <xsl:apply-templates select="@*"/>
      <!-- Add HKCU KeyPath -->
      <wix:RegistryValue Root="HKCU"
                         Key="Software\Netlogix\FinConsolidation\Files"
                         Name="{$cmpId}"
                         Type="integer"
                         Value="1"
                         KeyPath="yes"/>
      <!-- Copy children, but strip File KeyPath="yes" -->
      <xsl:for-each select="node()">
        <xsl:choose>
          <xsl:when test="self::wix:File and @KeyPath='yes'">
            <xsl:copy>
              <xsl:apply-templates select="@*[name()!='KeyPath']|node()"/>
            </xsl:copy>
          </xsl:when>
          <xsl:otherwise><xsl:apply-templates select="."/></xsl:otherwise>
        </xsl:choose>
      </xsl:for-each>
    </xsl:copy>
  </xsl:template>
</xsl:stylesheet>