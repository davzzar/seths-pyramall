<?xml version="1.0" encoding="UTF-8"?>
<tileset version="1.8" tiledversion="1.8.2" name="SmileyAnimated" tilewidth="64" tileheight="64" tilecount="8" columns="4">
 <image source="../SmileyAnimated.png" width="256" height="128"/>
 <tile id="0">
  <properties>
   <property name="AnimIsEntry" type="bool" value="true"/>
   <property name="AnimIsLoop" type="bool" value="true"/>
   <property name="AnimName" value="Idle"/>
  </properties>
  <animation>
   <frame tileid="0" duration="100"/>
   <frame tileid="1" duration="100"/>
   <frame tileid="2" duration="100"/>
   <frame tileid="3" duration="100"/>
   <frame tileid="2" duration="100"/>
  </animation>
 </tile>
 <tile id="4">
  <properties>
   <property name="AnimIsEntry" type="bool" value="false"/>
   <property name="AnimIsLoop" type="bool" value="true"/>
   <property name="AnimName" value="Jump"/>
  </properties>
  <animation>
   <frame tileid="4" duration="100"/>
   <frame tileid="5" duration="100"/>
   <frame tileid="6" duration="100"/>
   <frame tileid="7" duration="100"/>
   <frame tileid="6" duration="100"/>
  </animation>
 </tile>
</tileset>
