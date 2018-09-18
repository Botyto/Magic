require "Class"
require "CSharpParser"
require "DocGraph"
require "MarkdeepWriter"
BuildClasses()

local folders = { "../../Assets/" }

local parser = CSharpParser:new()
local graph = DocGraph:new()
local writer = MarkdeepWriter:new()

--graph:AddFolders(folders)
graph:AddFile(parser, "../../Assets/Magic/EnergyHolder.cs")
local class_map = graph:GenerateMapNode(function(n) return IsKindOf(n, "ClassNode") end)
class_map.title = "Classes"
class_map.summary = "All classes."
class_map.file_path = "map-Classes.md.html"
local main_page = graph:GenerateMainPage()

--save public
local output_folder = "../output/public/"
writer.required_tags = { public = true }
writer:ClearOutput(output_folder)
writer:Output(graph, output_folder)

--save private
local output_folder = "../output/private/"
writer.required_tags = { private = true }
writer:ClearOutput(output_folder)
writer:Output(graph, output_folder)
