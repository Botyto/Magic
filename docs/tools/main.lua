require "Class"
require "CSharpParser"
require "DocGraph"
require "MarkdeepWriter"
BuildClasses()

local folders = { "../../Assets/" }
local output_folder = "../output/"

local parser = CSharpParser:new()
local graph = DocGraph:new()
local writer = MarkdeepWriter:new()

--graph:AddFolders(folders)
graph:AddFile(parser, "../../Assets/Magic/EnergyHolder.cs")
local class_map = graph:GenerateMapNode(function(n) return IsKindOf(n, "ClassNode") end)
class_map.title = "Classes"
class_map.summary = "All classes."
class_map.file_path = "map-Classes.md.html"

writer:ClearOutput(output_folder)
writer:Output(graph, output_folder)
