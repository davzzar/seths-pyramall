<?xml version="1.0" encoding="UTF-8"?>
<tileset version="1.8" tiledversion="1.8.2" name="PlayerAnimated" tilewidth="32" tileheight="32" tilecount="128" columns="8">
 <image source="../PlayerAnimated.png" width="256" height="512"/>
 <tile id="0">
  <properties>
   <property name="AnimIsEntry" type="bool" value="true"/>
   <property name="AnimIsLoop" type="bool" value="false"/>
   <property name="AnimName" value="Spawn"/>
  </properties>
  <animation>
   <frame tileid="0" duration="120"/>
   <frame tileid="1" duration="120"/>
   <frame tileid="2" duration="120"/>
   <frame tileid="3" duration="120"/>
   <frame tileid="4" duration="120"/>
   <frame tileid="5" duration="120"/>
   <frame tileid="6" duration="120"/>
   <frame tileid="7" duration="120"/>
   <frame tileid="8" duration="120"/>
   <frame tileid="9" duration="120"/>
   <frame tileid="10" duration="120"/>
   <frame tileid="11" duration="120"/>
   <frame tileid="12" duration="120"/>
   <frame tileid="13" duration="120"/>
   <frame tileid="14" duration="120"/>
   <frame tileid="15" duration="120"/>
   <frame tileid="16" duration="120"/>
   <frame tileid="17" duration="120"/>
   <frame tileid="18" duration="120"/>
   <frame tileid="19" duration="120"/>
   <frame tileid="20" duration="120"/>
   <frame tileid="21" duration="120"/>
   <frame tileid="22" duration="120"/>
   <frame tileid="23" duration="120"/>
   <frame tileid="24" duration="120"/>
   <frame tileid="25" duration="120"/>
   <frame tileid="26" duration="120"/>
   <frame tileid="27" duration="120"/>
   <frame tileid="28" duration="120"/>
   <frame tileid="29" duration="120"/>
   <frame tileid="30" duration="120"/>
   <frame tileid="31" duration="120"/>
  </animation>
 </tile>
 <tile id="32">
  <properties>
   <property name="AnimIsEntry" type="bool" value="false"/>
   <property name="AnimIsLoop" type="bool" value="true"/>
   <property name="AnimName" value="Fall"/>
  </properties>
  <animation>
   <frame tileid="32" duration="100"/>
   <frame tileid="33" duration="100"/>
   <frame tileid="34" duration="100"/>
   <frame tileid="35" duration="100"/>
  </animation>
 </tile>
 <tile id="44">
  <properties>
   <property name="AnimIsEntry" type="bool" value="false"/>
   <property name="AnimIsLoop" type="bool" value="true"/>
   <property name="AnimName" value="Move"/>
  </properties>
  <animation>
   <frame tileid="44" duration="30"/>
   <frame tileid="45" duration="30"/>
   <frame tileid="46" duration="30"/>
   <frame tileid="47" duration="30"/>
   <frame tileid="48" duration="30"/>
   <frame tileid="49" duration="30"/>
   <frame tileid="50" duration="30"/>
   <frame tileid="51" duration="30"/>
   <frame tileid="52" duration="30"/>
   <frame tileid="53" duration="30"/>
   <frame tileid="54" duration="30"/>
   <frame tileid="55" duration="30"/>
   <frame tileid="56" duration="30"/>
   <frame tileid="57" duration="30"/>
   <frame tileid="58" duration="30"/>
   <frame tileid="59" duration="30"/>
  </animation>
 </tile>
 <tile id="64">
  <properties>
   <property name="AnimIsEntry" type="bool" value="false"/>
   <property name="AnimIsLoop" type="bool" value="false"/>
   <property name="AnimName" value="Jump"/>
  </properties>
  <animation>
   <frame tileid="64" duration="180"/>
   <frame tileid="64" duration="180"/>
   <frame tileid="64" duration="180"/>
   <frame tileid="64" duration="180"/>
  </animation>
 </tile>
 <tile id="72">
  <properties>
   <property name="AnimIsEntry" type="bool" value="false"/>
   <property name="AnimIsLoop" type="bool" value="true"/>
   <property name="AnimName" value="Idle"/>
  </properties>
  <animation>
   <frame tileid="72" duration="100"/>
  </animation>
 </tile>
 <tile id="80">
  <properties>
   <property name="AnimIsEntry" type="bool" value="false"/>
   <property name="AnimIsLoop" type="bool" value="false"/>
   <property name="AnimName" value="Land"/>
  </properties>
  <animation>
   <frame tileid="80" duration="300"/>
   <frame tileid="81" duration="300"/>
   <frame tileid="82" duration="300"/>
   <frame tileid="83" duration="300"/>
   <frame tileid="31" duration="300"/>
  </animation>
 </tile>
 <tile id="88">
  <properties>
   <property name="AnimIsEntry" type="bool" value="false"/>
   <property name="AnimIsLoop" type="bool" value="true"/>
   <property name="AnimName" value="Drown"/>
  </properties>
  <animation>
   <frame tileid="88" duration="48"/>
   <frame tileid="89" duration="48"/>
   <frame tileid="90" duration="48"/>
   <frame tileid="91" duration="48"/>
   <frame tileid="92" duration="48"/>
   <frame tileid="93" duration="48"/>
   <frame tileid="94" duration="48"/>
   <frame tileid="95" duration="48"/>
   <frame tileid="96" duration="48"/>
   <frame tileid="97" duration="48"/>
   <frame tileid="98" duration="48"/>
   <frame tileid="99" duration="48"/>
  </animation>
 </tile>
 <tile id="104">
  <properties>
   <property name="AnimIsEntry" type="bool" value="false"/>
   <property name="AnimIsLoop" type="bool" value="false"/>
   <property name="AnimName" value="Die"/>
  </properties>
  <animation>
   <frame tileid="104" duration="300"/>
   <frame tileid="104" duration="300"/>
   <frame tileid="104" duration="300"/>
  </animation>
 </tile>
 <tile id="112">
  <properties>
   <property name="AnimIsEntry" type="bool" value="false"/>
   <property name="AnimIsLoop" type="bool" value="false"/>
   <property name="AnimName" value="FoundItem"/>
  </properties>
  <animation>
   <frame tileid="63" duration="300"/>
  </animation>
 </tile>
 <tile id="113">
  <properties>
   <property name="AnimIsEntry" type="bool" value="false"/>
   <property name="AnimIsLoop" type="bool" value="false"/>
   <property name="AnimName" value="Trip"/>
  </properties>
  <animation>
   <frame tileid="41" duration="300"/>
  </animation>
 </tile>
</tileset>
