module Exercise_31_34

(* Exercise 3.1 *)

    type 'a bintree when 'a : comparison =
    | Leaf
    | Node of ('a * 'a bintree * 'a bintree);;

    let rec insert x =
        function 
        | Leaf                       -> Node (x, Leaf, Leaf)
        | Node (y, l, r) when x <= y -> Node (y, insert x l, r)
        | Node (y, l, r)             -> Node (y, l, insert x r);;

    let rec inOrder =
        function
        | Leaf           -> []
        | Node (x, l, r) -> inOrder l @ x::inOrder r;;

    (* TODO: write binarySort : 'a list -> 'a list when 'a : comparison *)
    let binarySort lst = lst |> List.fold (fun acc x -> insert x acc) Leaf |> inOrder;;

(* Exercise 3.2 *)

    (* TODO: write mapInOrder : ('a -> 'b) -> 'a bintree -> 'b bintree *)
    let mapInOrder f tree = 
        let rec traverser =
            function
            | Leaf           -> Leaf
            | Node (x, l, r) -> let left = traverser l
                                Node (f x, left, traverser r)
        traverser tree;;
    
    (* The mapInOrder will traverse the tree going down the left branches before going through the right branches.
       The mapPostOrder will traverse the tree the other way around, doing the right branches first before going down the left branches. *)

(* Exercise 3.3 *)

    (* TODO: write foldInOrder : ('a -> 'b -> 'b) -> 'b -> 'a bintree -> 'b *)
    let foldInOrder f n tree = inOrder tree |> List.fold (fun acc x -> f x acc) n;;