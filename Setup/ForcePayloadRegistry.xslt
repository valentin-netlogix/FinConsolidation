<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="1.0" 
  xmlns:xsl="http://www.w3.org/1999/XSL/Transform" 
  xmlns:wix="http://wixtoolset.org/schemas/v4/wxs"
  xmlns="http://wixtoolset.org/schemas/v4/wxs"
  exclude-result-prefixes="wix">

  <xsl:output method="xml" indent="yes" />

  <!-- Copy everything by default -->
  <xsl:template match="@*|node()">
    <xsl:copy>
      <xsl:apply-templates select="@*|node()"/>
    </xsl:copy>
  </xsl:template>

  <!-- Match Components inside the harvested fragments -->
  <xsl:template match="wix:Component">
    <xsl:copy>
      <xsl:apply-templates select="@*[name()!='Guid']"/>
      <xsl:attribute name="Guid"></xsl:attribute>
      <RegistryValue Root="HKCU" Key="Software\Netlogix\FinConsolidation\Components" Name="{@Id}" Type="string" Value="" KeyPath="yes" />
      <RemoveFolder Id="Remove_{@Id}" On="uninstall" />
      <xsl:apply-templates select="node()"/>
    </xsl:copy>
  </xsl:template>

  <!-- 
    Fix for WIX0267: 
    When we create a cleanup component for a directory, we must also 
    add a reference to it in the ComponentGroup so it's not orphaned.
  -->
  <xsl:template match="wix:ComponentGroup">
    <xsl:copy>
      <xsl:apply-templates select="@*|node()"/>
      <!-- Add references to all directory cleanup components created below -->
      <xsl:for-each select="//wix:Directory[not(wix:Component)]">
        <ComponentRef Id="Cleanup_{@Id}" />
      </xsl:for-each>
    </xsl:copy>
  </xsl:template>

  <!-- Handle directories that need cleanup components -->
  <xsl:template match="wix:Directory">
    <xsl:copy>
      <xsl:apply-templates select="@*|node()"/>
      <xsl:if test="not(wix:Component)">
         <Component Id="Cleanup_{@Id}" Guid="">
            <RegistryValue Root="HKCU" Key="Software\Netlogix\FinConsolidation\DirectoryCleanup" Name="{@Id}" Type="string" Value="" KeyPath="yes" />
            <RemoveFolder Id="RemoveDir_{@Id}" On="uninstall" />
         </Component>
      </xsl:if>
    </xsl:copy>
  </xsl:template>

  <xsl:template match="wix:File/@KeyPath" />

</xsl:stylesheet>