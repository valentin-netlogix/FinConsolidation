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

  <!-- For each harvested Component, add a RemoveFolder entry.
       Without Directory=..., it removes the component's directory. -->
  <xsl:template match="wix:Component">
    <xsl:copy>
      <xsl:apply-templates select="@*|node()"/>
      <wix:RemoveFolder On="uninstall"/>
    </xsl:copy>
  </xsl:template>
</xsl:stylesheet>