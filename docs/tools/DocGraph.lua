require "DocsMapNode"
require "MainPageNode"

Class.DocGraph = {
	nodes = false,

	stage = false,
}

function DocGraph:__ctor()
	self.nodes = { }
end

function DocGraph:AddFolders(folders)
	for i,folder in ipairs(folders) do
		self:AddFolder(folder)
	end
end

function DocGraph:AddFolder(folder)
	local files = io.listfiles(folder)
	for i,file in ipairs(files) do
		self:AddFile(file)
	end
end

function DocGraph:AddFile(parser, file)
	if self.stage then
		print("!WARNING! Map/Main nodes have been generated and adding new files is not suggested!")
	end

	local new_nodes = parser:ParseFile(file)
	self:AddNodes(new_nodes)
end

function DocGraph:GenerateMapNode(filter)
	if self.stage == "Main" then
		print("!WARNING! Main node has been generated and adding new maps is not suggested")
	end
	self.stage = "Map"

	local map = DocsMapNode:new()
	for i,node in ipairs(self.nodes) do
		if filter(node) then
			table.insert(map.elements, node)
		end
	end
	self:AddNodes({ map })
	return map
end

function DocGraph:GenerateMainPage()
	if self.stage == "Main" then
		print("!WARNING! Main node has already been generated")
	end
	self.stage = "Main"

	local main_page = MainPageNode:new()
	main_page.maps = { }
	for i,node in ipairs(self.nodes) do
		if IsKindOf(node, "DocsMapNode") then
			table.insert(main_page.maps, node)
		end
	end

	self:AddNodes({ main_page })
	return main_page
end

function DocGraph:AddNodes(new_nodes)
	--try merge with an old node or just add it
	for i,new_node in ipairs(new_nodes) do
		local merge_success
		for j,old_node in ipairs(self.nodes) do
			merge_success = old_node:TryMergeNode(new_node)
			if merge_success then break end
		end

		if not merge_success then
			table.insert(self.nodes, new_node)
		end
	end

	self:UpdateConnections()
end

function DocGraph:UpdateConnections()
	for i,node_1 in ipairs(self.nodes) do
		for j,node_2 in ipairs(self.nodes) do
			if node_1 ~= node_2 then
				node_1:UpdateConnectionsWithNode(node_2)
				node_2:UpdateConnectionsWithNode(node_1)
			end
		end
	end
end

function DocGraph:FindNode(title)
	for i,node in ipairs(self.nodes) do
		if node.title == title then
			return node
		end
	end
end
