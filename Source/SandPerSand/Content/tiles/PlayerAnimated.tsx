<?xml version="1.0" encoding="UTF-8"?>
<tileset version="1.8" tiledversion="1.8.2" name="PlayerAnimated" tilewidth="32" tileheight="32" tilecount="64" columns="8">
 <image source="../PlayerAnimated.png" width="256" height="256"/>
 <tile id="0">
  <properties>
   <property name="AnimIsEntry" type="bool" value="true"/>
   <property name="AnimIsLoop" type="bool" value="false"/>
   <property name="AnimName" value="Spawn"/>
  </properties>
  <animation>
   <frame tileid="0" duration="150"/>
   <frame tileid="1" duration="150"/>
   <frame tileid="2" duration="150"/>
   <frame tileid="3" duration="150"/>
   <frame tileid="4" duration="150"/>
   <frame tileid="5" duration="150"/>
   <frame tileid="6" duration="150"/>
   <frame tileid="7" duration="150"/>
   <frame tileid="8" duration="150"/>
   <frame tileid="9" duration="150"/>
   <frame tileid="10" duration="150"/>
   <frame tileid="11" duration="150"/>
   <frame tileid="12" duration="150"/>
   <frame tileid="13" duration="150"/>
   <frame tileid="14" duration="150"/>
   <frame tileid="15" duration="150"/>
   <frame tileid="16" duration="150"/>
   <frame tileid="17" duration="150"/>
   <frame tileid="18" duration="150"/>
   <frame tileid="19" duration="150"/>
   <frame tileid="20" duration="150"/>
   <frame tileid="21" duration="150"/>
   <frame tileid="22" duration="150"/>
   <frame tileid="23" duration="150"/>
   <frame tileid="24" duration="150"/>
   <frame tileid="25" duration="150"/>
   <frame tileid="26" duration="150"/>
   <frame tileid="27" duration="150"/>
   <frame tileid="28" duration="150"/>
   <frame tileid="29" duration="150"/>
   <frame tileid="30" duration="150"/>
   <frame tileid="31" duration="150"/>
  </animation>
 </tile>
 <tile id="32">
  <properties>
   <property name="AnimIsEntry" type="bool" value="false"/>
   <property name="AnimIsLoop" type="bool" value="true"/>
   <property name="AnimName" value="Move"/>
  </properties>
  <animation>
   <frame tileid="32" duration="100"/>
   <frame tileid="33" duration="100"/>
   <frame tileid="34" duration="100"/>
   <frame tileid="35" duration="100"/>
  </animation>
 </tile>
 <tile id="40">
  <properties>
   <property name="AnimIsEntry" type="bool" value="false"/>
   <property name="AnimIsLoop" type="bool" value="true"/>
   <property name="AnimName" value="Idle"/>
  </properties>
  <animation>
   <frame tileid="31" duration="100"/>
  </animation>
 </tile>
</tileset>
